using System;

namespace Torrent.BEncoding.Serialization
{
    public static partial class BEncodingSerializer
    {
        public static bool TryParse(ReadOnlySpan<byte> data, out ReadOnlySpan<byte> utf8value)
        {
            var reader = new Utf8BEncodingReader(data);

            if (reader.Read() &&
                reader.TokenType == BEncodingTokenType.String &&
                reader.IsEmpty)
            {
                utf8value = reader.ValueSpan;
                return true;
            }

            utf8value = default;
            return false;
        }

        public static bool TryParse(ReadOnlySpan<byte> data, out string value)
        {
            var reader = new Utf8BEncodingReader(data);

            if (reader.Read() &&
                reader.TokenType == BEncodingTokenType.String &&
                reader.IsEmpty &&
                reader.TryGet(out value))
            {
                return true;
            }

            value = null;
            return false;
        }
    }
}
