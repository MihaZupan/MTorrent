using System;
using System.Numerics;

namespace Torrent.BEncoding.Serialization
{
    public static partial class BEncodingSerializer
    {
        public static bool TryParse(ReadOnlySpan<byte> data, out int value)
        {
            var reader = new Utf8BEncodingReader(data);

            if (reader.Read() &&
                reader.TokenType == BEncodingTokenType.Integer &&
                reader.IsEmpty &&
                reader.TryGet(out value))
            {
                return true;
            }

            value = 0;
            return false;
        }

        public static bool TryParse(ReadOnlySpan<byte> data, out uint value)
        {
            var reader = new Utf8BEncodingReader(data);

            if (reader.Read() &&
                reader.TokenType == BEncodingTokenType.Integer &&
                reader.IsEmpty &&
                reader.TryGet(out value))
            {
                return true;
            }

            value = 0;
            return false;
        }

        public static bool TryParse(ReadOnlySpan<byte> data, out long value)
        {
            var reader = new Utf8BEncodingReader(data);

            if (reader.Read() &&
                reader.TokenType == BEncodingTokenType.Integer &&
                reader.IsEmpty &&
                reader.TryGet(out value))
            {
                return true;
            }

            value = 0;
            return false;
        }

        public static bool TryParse(ReadOnlySpan<byte> data, out ulong value)
        {
            var reader = new Utf8BEncodingReader(data);

            if (reader.Read() &&
                reader.TokenType == BEncodingTokenType.Integer &&
                reader.IsEmpty &&
                reader.TryGet(out value))
            {
                return true;
            }

            value = 0;
            return false;
        }

        public static bool TryParse(ReadOnlySpan<byte> data, out BigInteger value)
        {
            var reader = new Utf8BEncodingReader(data);

            if (reader.Read() &&
                reader.TokenType == BEncodingTokenType.Integer &&
                reader.IsEmpty &&
                reader.TryGet(out value))
            {
                return true;
            }

            value = BigInteger.Zero;
            return false;
        }
    }
}
