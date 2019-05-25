using System.Text;
using Torrent.BEncoding;
using Xunit;

namespace Torrent.Tests.BEncoding.Reader
{
    public static class ReaderTests
    {
        public static Utf8BEncodingReader GetReader(string bencoded)
        {
            byte[] bencodedBytes = Encoding.UTF8.GetBytes(bencoded);

            var reader = new Utf8BEncodingReader(bencodedBytes);

            Assert.Equal(BEncodingTokenType.None, reader.TokenType);

            return reader;
        }
    }
}
