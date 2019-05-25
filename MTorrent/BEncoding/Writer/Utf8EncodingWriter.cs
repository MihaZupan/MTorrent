using System;
using System.Buffers;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;

namespace Torrent.BEncoding
{
    // Based on Utf8JsonWriter from System.Text.Json
    public partial class Utf8BEncodingWriter
    {
        private const int DefaultGrowthSize = 4096;
        private const int InitialGrowthSize = 256;

        private IBufferWriter<byte> _output;
        private Stream _stream;
        private ArrayBufferWriter<byte> _arrayBufferWriter;

        private Memory<byte> _memory;
        public int BytesPending { get; private set; }
        public int BytesCommitted { get; private set; }

        public Utf8BEncodingWriter(Stream destination)
        {
            _stream = destination;
            _arrayBufferWriter = new ArrayBufferWriter<byte>();

            _output = null;
            _memory = default;

            BytesPending = 0;
            BytesCommitted = 0;
        }

        public Utf8BEncodingWriter(IBufferWriter<byte> bufferWriter)
        {
            _output = bufferWriter;

            _stream = null;
            _arrayBufferWriter = null;
            _memory = default;

            BytesPending = 0;
            BytesCommitted = 0;
        }

        private void Grow(int requiredSize)
        {
            if (_memory.Length == 0)
            {
                FirstGrow(requiredSize);
                return;
            }

            int sizeHint = Math.Max(DefaultGrowthSize, requiredSize);

            Debug.Assert(BytesPending != 0);

            if (_stream != null)
            {
                Debug.Assert(_arrayBufferWriter != null);

                _arrayBufferWriter.Advance(BytesPending);

                Debug.Assert(BytesPending == _arrayBufferWriter.WrittenCount);

                _stream.Write(_arrayBufferWriter.WrittenSpan);

                _arrayBufferWriter.Clear();

                _memory = _arrayBufferWriter.GetMemory(sizeHint);

                Debug.Assert(_memory.Length >= sizeHint);
            }
            else
            {
                Debug.Assert(_output != null);

                _output.Advance(BytesPending);

                _memory = _output.GetMemory(sizeHint);

                if (_memory.Length < sizeHint)
                {
                    // ToDo - replace this
                    throw new Exception("Need more Span size");
                }
            }

            BytesCommitted += BytesPending;
            BytesPending = 0;
        }
        private void FirstGrow(int requiredSize)
        {
            Debug.Assert(_memory.Length == 0);
            Debug.Assert(BytesPending == 0);
            Debug.Assert(BytesCommitted == 0);

            int sizeHint = Math.Max(InitialGrowthSize, requiredSize);

            if (_stream != null)
            {
                Debug.Assert(_arrayBufferWriter != null);
                Debug.Assert(BytesPending == _arrayBufferWriter.WrittenCount);
                _memory = _arrayBufferWriter.GetMemory(sizeHint);
                Debug.Assert(_memory.Length >= sizeHint);
            }
            else
            {
                Debug.Assert(_output != null);
                _memory = _output.GetMemory(sizeHint);

                if (_memory.Length < sizeHint)
                {
                    // ToDo - replace this
                    throw new Exception("Need more Span size");
                }
            }
        }

        public void Flush()
        {
            if (_stream != null)
            {
                Debug.Assert(_arrayBufferWriter != null);
                if (BytesPending != 0)
                {
                    _arrayBufferWriter.Advance(BytesPending);
                    Debug.Assert(BytesPending == _arrayBufferWriter.WrittenCount);
                    _stream.Write(_arrayBufferWriter.WrittenSpan);
                    _arrayBufferWriter.Clear();
                }
                _stream.Flush();
            }
            else
            {
                Debug.Assert(_output != null);
                if (BytesPending != 0)
                {
                    _output.Advance(BytesPending);
                }
            }

            _memory = default;
            BytesCommitted += BytesPending;
            BytesPending = 0;
        }
        public async Task FlushAsync()
        {
            if (_stream != null)
            {
                Debug.Assert(_arrayBufferWriter != null);
                if (BytesPending != 0)
                {
                    _arrayBufferWriter.Advance(BytesPending);
                    Debug.Assert(BytesPending == _arrayBufferWriter.WrittenCount);
                    await _stream.WriteAsync(_arrayBufferWriter.WrittenMemory).ConfigureAwait(false);
                    _arrayBufferWriter.Clear();
                }
                await _stream.FlushAsync().ConfigureAwait(false);
            }
            else
            {
                Debug.Assert(_output != null);
                if (BytesPending != 0)
                {
                    _output.Advance(BytesPending);
                }
            }

            _memory = default;
            BytesCommitted += BytesPending;
            BytesPending = 0;
        }


        public void WriteDictionaryStart()
        {
            if (_memory.Length - BytesPending < 2)
                Grow(2);

            _memory.Span[BytesPending++] = BEncodingConstants.OpenDictionary;
        }

        public void WriteListStart()
        {
            if (_memory.Length - BytesPending < 2)
                Grow(2);

            _memory.Span[BytesPending++] = BEncodingConstants.OpenList;
        }

        public void WriteEnd()
        {
            if (_memory.Length == BytesPending)
                Grow(1);

            _memory.Span[BytesPending++] = BEncodingConstants.End;
        }
        public void WriteEnd(int count = 1)
        {
            if (_memory.Length - BytesPending < count)
                Grow(count);

            var span = _memory.Span;

            while (--count >= 0)
                span[BytesPending++] = BEncodingConstants.End;
        }
    }
}
