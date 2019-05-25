using System;
using System.Buffers.Text;
using System.Diagnostics;
using System.Numerics;
using System.Text;

namespace Torrent.BEncoding
{
    public ref struct Utf8BEncodingReader
    {
        private ReadOnlySpan<byte> _buffer;
        public int Consumed { get; private set; }
#if DEBUG
        private bool _corruptedState;
#endif
        public bool IsEmpty => (uint)Consumed >= (uint)_buffer.Length;

        public BEncodingTokenType TokenType { get; private set; }
        public ReadOnlySpan<byte> ValueSpan { get; private set; }

        public Utf8BEncodingReader(ReadOnlySpan<byte> data)
        {
            _buffer = data;

            Consumed = 0;

            TokenType = BEncodingTokenType.None;
            ValueSpan = default;

#if DEBUG
            _corruptedState = false;
#endif
        }

#if DEBUG
        public bool Read()
        {
            Debug.Assert(!_corruptedState, "Read should not be called after it returned false");
            bool ret = ReadDebug();
            if (!ret) _corruptedState = true;
            return ret;
        }

        private bool ReadDebug()
#else
        public bool Read()
#endif
        {
            if ((uint)Consumed >= (uint)_buffer.Length)
                return false;

            byte first = _buffer[Consumed];

            if (first == BEncodingConstants.End)
            {
                TokenType = BEncodingTokenType.End;
                Consumed++;
                return true;
            }
            else return ConsumeValue(first);
        }

        private bool ConsumeValue(byte first)
        {
            if (first == BEncodingConstants.OpenList)
            {
                TokenType = BEncodingTokenType.StartList;
            }
            else if (first == BEncodingConstants.OpenDictionary)
            {
                TokenType = BEncodingTokenType.StartDictionary;
            }
            else if (first == BEncodingConstants.OpenInteger)
            {
                return ConsumeInteger();
            }
            else return ConsumeString(first);

            Consumed++;
            return true;
        }

        private bool ConsumeInteger()
        {
            Debug.Assert(_buffer[Consumed] == BEncodingConstants.OpenInteger);

            ReadOnlySpan<byte> localSpan = _buffer.Slice(Consumed + 1);

            int index = localSpan.IndexOf(BEncodingConstants.End);
            if (index == -1)
                return false;

            Consumed += index + 2;
            ValueSpan = localSpan.Slice(0, index);
            TokenType = BEncodingTokenType.Integer;
            return true;
        }

        private bool ConsumeString(byte first)
        {
            if (!BEncodingHelpers.IsDigit(first))
                return false;

            ReadOnlySpan<byte> localSpan = _buffer.Slice(Consumed);

            int index = localSpan.IndexOf(BEncodingConstants.ColonSeparator);
            if (index == -1)
                return false;

            if (first == '0')
            {
                if (index != 1)
                    return false;

                Consumed += 2;
                ValueSpan = default;
            }
            else
            {
                if (!Utf8Parser.TryParse(localSpan.Slice(0, index), out int length, out int bytesConsumed) || bytesConsumed != index)
                    return false;

                int consumed = Consumed + index + 1 + length;

                if ((uint)consumed > (uint)_buffer.Length)
                    return false;

                Consumed = consumed;

                ValueSpan = localSpan.Slice(index + 1, length);
            }

            TokenType = BEncodingTokenType.String;
            return true;
        }

        public bool Skip(int count)
        {
            for (int i = 0; i < count; i++)
                if (!Read())
                    return false;
            return true;
        }

        public int TryGetLowercase(Span<char> charBuffer, Span<char> lowerCaseBuffer)
        {
            Debug.Assert(charBuffer.Length == lowerCaseBuffer.Length);

            if (ValueSpan.Length > charBuffer.Length)
                return -1;

            try
            {
                int count = BEncodingHelpers.UTF8.GetChars(ValueSpan, charBuffer);
                return ((ReadOnlySpan<char>)charBuffer).Slice(0, count).ToLowerInvariant(lowerCaseBuffer);
            }
            catch
            {
                return -1;
            }
        }
        public bool TryGet(out string value)
        {
            value = null;
            try
            {
                value = BEncodingHelpers.UTF8.GetString(ValueSpan);
                return true;
            }
            catch
            {
                return false;
            }
        }
        public bool TryGet(out int value)
        {
            value = 0;

            if (ValueSpan.Length == 0)
                return true;

            if (ValueSpan.Length > 11) // Guaranteed malformed / overflow
                return false;

            if (!Utf8Parser.TryParse(ValueSpan, out value, out int bytesConsumed) || bytesConsumed != ValueSpan.Length)
                return false;

            return value > 0
                ? ValueSpan[0] != '0' && ValueSpan[0] != '+'
                : value == 0
                    ? ValueSpan.Length == 1 && ValueSpan[0] == '0'
                    : ValueSpan[1] != '0';
        }
        public bool TryGet(out long value)
        {
            value = 0;

            if (ValueSpan.Length == 0)
                return true;

            if (ValueSpan.Length > 20) // Guaranteed malformed / overflow
                return false;

            if (!Utf8Parser.TryParse(ValueSpan, out value, out int bytesConsumed) || bytesConsumed != ValueSpan.Length)
                return false;

            return value > 0
                ? ValueSpan[0] != '0' && ValueSpan[0] != '+'
                : value == 0
                    ? ValueSpan.Length == 1 && ValueSpan[0] == '0'
                    : ValueSpan[1] != '0';
        }
        public bool TryGet(out uint value)
        {
            value = 0;

            if (ValueSpan.Length == 0)
                return true;

            if (ValueSpan.Length > 10) // Guaranteed malformed / overflow
                return false;

            if (!Utf8Parser.TryParse(ValueSpan, out value, out int bytesConsumed) || bytesConsumed != ValueSpan.Length)
                return false;

            return value == 0
                ? ValueSpan.Length == 1 && ValueSpan[0] == '0'
                : ValueSpan[0] != '0' && ValueSpan[0] != '+';
        }
        public bool TryGet(out ulong value)
        {
            value = 0;

            if (ValueSpan.Length == 0)
                return true;

            if (ValueSpan.Length > 20) // Guaranteed malformed / overflow
                return false;

            if (!Utf8Parser.TryParse(ValueSpan, out value, out int bytesConsumed) || bytesConsumed != ValueSpan.Length)
                return false;

            return value == 0
                ? ValueSpan.Length == 1 && ValueSpan[0] == '0'
                : ValueSpan[0] != '0' && ValueSpan[0] != '+';
        }
        public bool TryGet(out BigInteger value)
        {
            // Fast-path "smaller" integers
            if (ValueSpan.Length < 19)
            {
                if (TryGet(out long longValue))
                {
                    value = new BigInteger(longValue);
                    return true;
                }

                goto ReturnFalse;
            }

            // It's a big number
            // We can still get some perf by building the BigInteger by sizeof(ulong)

            // Possible perf improvement:
            // manually building up the bits representation and constructing a BigInteger only once for huge values

            // We can start with a 'long' section to pick up the sign
            if (!Utf8Parser.TryParse(ValueSpan.Slice(0, 18), out long startingNumber, out int bytesConsumed) || bytesConsumed != 18)
            {
                goto ReturnFalse;
            }

            if (startingNumber > -10_000_000_000_000_000L &&
                startingNumber < 100_000_000_000_000_000L)
                goto ReturnFalse;

            value = new BigInteger(startingNumber < 0 ? -startingNumber : startingNumber);

            ReadOnlySpan<byte> localSpan = ValueSpan.Slice(18);
            do
            {
                int len = Math.Min(localSpan.Length, 19);
                if (!Utf8Parser.TryParse(localSpan.Slice(0, len), out ulong segment, out bytesConsumed) || bytesConsumed != len)
                {
                    goto ReturnFalse;
                }

                localSpan = localSpan.Slice(len);

                ulong shift = len == 19
                    ? 10_000_000_000_000_000_000u
                    : BEncodingHelpers.Pow10(len);

                value = value * shift + segment;
            }
            while (localSpan.Length > 0);

            if (startingNumber < 0)
                value = -value;

            return true;

        ReturnFalse:
            value = BigInteger.Zero;
            return false;
        }
    }
}
