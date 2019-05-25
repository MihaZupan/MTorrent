// Copyright (c) Miha Zupan. All rights reserved.
// This file is licensed under the BSD-Clause 2 license.
// See the license.txt file in the project root for more information.
using System;
using System.Buffers.Text;
using System.Diagnostics;
using System.Numerics;

namespace MTorrent.BEncoding
{
    public partial class Utf8BEncodingWriter
    {
        // ToDo benchmark - fast-path for small values?

        public void Write(int value)
        {
            // 13 is the worst-case required size ('i', 'e', - and 10 digits)
            const int required = 13;

            if (_memory.Length - BytesPending < required)
                Grow(required);

            var span = _memory.Span;

            span[BytesPending++] = BEncodingConstants.OpenInteger;

            bool success = Utf8Formatter.TryFormat(value, span.Slice(BytesPending), out int written);
            Debug.Assert(success);
            BytesPending += written;

            span[BytesPending++] = BEncodingConstants.End;
        }

        public void Write(uint value)
        {
            // 12 is the worst-case required size ('i', 'e' and 10 digits)
            const int required = 12;

            if (_memory.Length - BytesPending < required)
                Grow(required);

            var span = _memory.Span;

            span[BytesPending++] = BEncodingConstants.OpenInteger;

            bool success = Utf8Formatter.TryFormat(value, span.Slice(BytesPending), out int written);
            Debug.Assert(success);
            BytesPending += written;

            span[BytesPending++] = BEncodingConstants.End;
        }

        public void Write(long value)
        {
            // 22 is the worst-case required size ('i', 'e', - and 19 digits)
            const int required = 22;

            if (_memory.Length - BytesPending < required)
                Grow(required);

            var span = _memory.Span;

            span[BytesPending++] = BEncodingConstants.OpenInteger;

            bool success = Utf8Formatter.TryFormat(value, span.Slice(BytesPending), out int written);
            Debug.Assert(success);
            BytesPending += written;

            span[BytesPending++] = BEncodingConstants.End;
        }

        public void Write(ulong value)
        {
            // 22 is the worst-case required size ('i', 'e' and 20 digits)
            const int required = 22;

            if (_memory.Length - BytesPending < required)
                Grow(required);

            var span = _memory.Span;

            span[BytesPending++] = BEncodingConstants.OpenInteger;

            bool success = Utf8Formatter.TryFormat(value, span.Slice(BytesPending), out int written);
            Debug.Assert(success);
            BytesPending += written;

            span[BytesPending++] = BEncodingConstants.End;
        }

        public void Write(BigInteger value)
        {
            bool negative = value.Sign < 0;
            if (negative) value = -value;
            int digitCount = value.IsZero ? 1 : (int)Math.Truncate(BigInteger.Log10(value)) + 1;
            int required = digitCount + 2 + (negative ? 1 : 0);

            if (_memory.Length - BytesPending < required)
                Grow(required);

            var span = _memory.Span;

            span[BytesPending++] = BEncodingConstants.OpenInteger;

            if (negative)
                span[BytesPending++] = BEncodingConstants.NegativeSign;

            Span<char> charBuffer = digitCount <= 128 ? stackalloc char[digitCount] : new char[digitCount];
            bool success = value.TryFormat(charBuffer, out int charsWritten);
            Debug.Assert(success);
            Debug.Assert(charsWritten == digitCount);

            int bytesWritten = BEncodingHelpers.UTF8.GetBytes(charBuffer, span.Slice(BytesPending));
            Debug.Assert(bytesWritten == charsWritten);
            BytesPending += bytesWritten;

            span[BytesPending++] = BEncodingConstants.End;
        }
    }
}
