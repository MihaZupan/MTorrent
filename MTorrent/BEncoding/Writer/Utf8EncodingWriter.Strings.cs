// Copyright (c) Miha Zupan. All rights reserved.
// This file is licensed under the BSD-Clause 2 license.
// See the license.txt file in the project root for more information.
using System;
using System.Buffers.Text;
using System.Diagnostics;
using System.Threading.Tasks;

namespace MTorrent.BEncoding
{
    public partial class Utf8BEncodingWriter
    {
        // ToDo benchmark - split writes into chunks?

        public async Task WriteAsync(Memory<byte> bytes)
        {
            if (_stream is null)
            {
                Write(bytes.Span);
                return;
            }

            int required = BEncodingHelpers.DigitCount(bytes.Length);

            if (_memory.Length - BytesPending < required)
                Grow(required);

            bool success = Utf8Formatter.TryFormat(bytes.Length, _memory.Span.Slice(BytesPending), out int written);
            Debug.Assert(success);
            Debug.Assert(written == required);
            BytesPending += written;

            _arrayBufferWriter.Advance(BytesPending);
            Debug.Assert(BytesPending == _arrayBufferWriter.WrittenCount);
            await _stream.WriteAsync(_arrayBufferWriter.WrittenMemory).ConfigureAwait(false);
            _arrayBufferWriter.Clear();
            _memory = _arrayBufferWriter.GetMemory(DefaultGrowthSize);

            BytesCommitted += BytesPending + bytes.Length;
            BytesPending = 0;

            while (bytes.Length > 0)
            {
                int chunkSize = Math.Min(bytes.Length, 4096);
                await _stream.WriteAsync(bytes.Slice(0, chunkSize)).ConfigureAwait(false);
                bytes = bytes.Slice(chunkSize);
            }

            await _stream.FlushAsync().ConfigureAwait(false);
        }

        public void Write(ReadOnlySpan<byte> bytes)
        {
            // ToDo - use bytes.Length + 11 here as the worst-case?
            // ASCII length, colon separator, bytes
            int required = bytes.Length + BEncodingHelpers.DigitCount(bytes.Length) + 1;

            if (_memory.Length - BytesPending < required)
                Grow(required);

            var span = _memory.Slice(BytesPending).Span;

            // ToDo - is this fast enough?
            bool success = Utf8Formatter.TryFormat(bytes.Length, span, out int written);
            Debug.Assert(success);
            Debug.Assert(written == BEncodingHelpers.DigitCount(bytes.Length));

            span[written] = BEncodingConstants.ColonSeparator;

            bytes.CopyTo(span.Slice(written + 1));

            BytesPending += required;
        }

        public void Write(ReadOnlySpan<char> chars)
        {
            // ToDo benchmark - tweaking numbers ...

            // Strategy:
            // 1. For values where length [0, 3] or [10, 33] we know the length of the ascii prefix
            // 2. For lengths [4, 6] or [34, 60], require worst-case buffer size but assume length
            // 3. Otherwise fall-back to GetByteCount

            // For strings above the length of 60, use the GetByteCount overload
            // Saving a GetByteCount call is not worth the potential over-allocation overhead

            if (chars.Length > 60)
            {
                WriteString_UnknownLength(chars);
                return;
            }

            int digitCount;
            bool fastPath;
            if (chars.Length < 10) { digitCount = 1; fastPath = chars.Length <= 3; }
            else { digitCount = 2; fastPath = chars.Length <= 33; }

            if (fastPath)
            {
                WriteString_KnownLengthDigitCount(chars, digitCount);
            }
            else if (chars.Length <= 6 || digitCount == 2)
            {
                WriteString_AssumedLengthDigitCount(chars, digitCount);
            }
            else
            {
                WriteString_UnknownLength(chars);
            }
        }

        private void WriteString_KnownLengthDigitCount(ReadOnlySpan<char> chars, int lengthDigitCount)
        {
            // ASCII length, colon separator, bytes (could expand up to 3x)
            int required = lengthDigitCount + 1 + chars.Length * BEncodingConstants.MaxExpansionFactorWhileTranscoding;

            if (_memory.Length - BytesPending < required)
                Grow(required);

            var span = _memory.Slice(BytesPending).Span;

            // Write the UTF8 bytes first - we don't know the exact length yet
            int bytesWritten = BEncodingHelpers.UTF8.GetBytes(chars, span.Slice(lengthDigitCount + 1));
            Debug.Assert(lengthDigitCount == BEncodingHelpers.DigitCount(bytesWritten));

            bool success = Utf8Formatter.TryFormat(bytesWritten, span, out int written);
            Debug.Assert(success);
            Debug.Assert(written == lengthDigitCount);

            span[written] = BEncodingConstants.ColonSeparator;

            BytesPending += bytesWritten + written + 1;
        }

        private void WriteString_AssumedLengthDigitCount(ReadOnlySpan<char> chars, int lengthDigitCount)
        {
            // ASCII length, 1 potential length extension, colon separator, bytes (could expand up to 3x)
            int required = lengthDigitCount + 2 + chars.Length * BEncodingConstants.MaxExpansionFactorWhileTranscoding;

            if (_memory.Length - BytesPending < required)
                Grow(required);

            var span = _memory.Slice(BytesPending).Span;

            // Write the UTF8 bytes first - we don't know the exact length yet
            var utf8Span = span.Slice(lengthDigitCount + 1);
            int bytesWritten = BEncodingHelpers.UTF8.GetBytes(chars, utf8Span);

            bool success = Utf8Formatter.TryFormat(bytesWritten, span, out int written);
            Debug.Assert(success);
            Debug.Assert(written == lengthDigitCount || written == lengthDigitCount + 1);

            if (BEncodingHelpers.DigitCount(bytesWritten) != lengthDigitCount)
            {
                // Unlikely case, writing the ColonSeparator would overwrite the first utf8 byte
                // Shift all the written bytes one place to the right
                // This is slow as it boils down to a P/Invoke because the buffers are overlapping
                utf8Span.Slice(0, bytesWritten).CopyTo(utf8Span.Slice(1, bytesWritten));

                // ToDo benchmark - stackalloc a temp buffer and do two copies vs P/Invoke
            }

            span[written] = BEncodingConstants.ColonSeparator;

            BytesPending += bytesWritten + written + 1;
        }

        private void WriteString_UnknownLength(ReadOnlySpan<char> chars)
        {
            int byteCount = BEncodingHelpers.UTF8.GetByteCount(chars);

            // ToDo - use knownByteCount + 11 here as the worst-case?
            // ASCII length, colon separator, bytes
            int required = byteCount + BEncodingHelpers.DigitCount(byteCount) + 1;

            if (_memory.Length - BytesPending < required)
                Grow(required);

            var span = _memory.Slice(BytesPending).Span;

            // ToDo - is this fast enough?
            bool success = Utf8Formatter.TryFormat(byteCount, span, out int written);
            Debug.Assert(success);
            Debug.Assert(written == BEncodingHelpers.DigitCount(byteCount));

            span[written] = BEncodingConstants.ColonSeparator;

            int bytesWritten = BEncodingHelpers.UTF8.GetBytes(chars, span.Slice(written + 1));
            Debug.Assert(bytesWritten == byteCount);

            BytesPending += required;
        }

        public void WriteASCIIUnchecked(ReadOnlySpan<char> ascii)
        {
            Debug.Assert(BEncodingHelpers.UTF8.GetByteCount(ascii) == ascii.Length, "Non ASCII chars found");

            // ToDo - use bytes.Length + 11 here as the worst-case?
            // ASCII length, colon separator, bytes
            int required = ascii.Length + BEncodingHelpers.DigitCount(ascii.Length) + 1;

            if (_memory.Length - BytesPending < required)
                Grow(required);

            var span = _memory.Slice(BytesPending).Span;

            // ToDo - is this fast enough?
            bool success = Utf8Formatter.TryFormat(ascii.Length, span, out int written);
            Debug.Assert(success);
            Debug.Assert(written == BEncodingHelpers.DigitCount(ascii.Length));

            span[written] = BEncodingConstants.ColonSeparator;

            int bytesWritten = BEncodingHelpers.ASCII.GetBytes(ascii, span.Slice(written + 1));
            Debug.Assert(bytesWritten == ascii.Length);

            BytesPending += required;
        }
    }
}
