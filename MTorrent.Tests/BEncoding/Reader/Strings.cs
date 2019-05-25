using System.Text;
using Torrent.BEncoding;
using Xunit;

namespace Torrent.Tests.BEncoding.Reader
{
    public class Strings
    {
        [Theory]
        [InlineData("")]
        [InlineData(" ")]
        [InlineData("Hello")]
        [InlineData(" Hello ")]
        [InlineData("Hello world!")]
        [InlineData("\"Hello World!\"")]
        [InlineData("München")]
        [InlineData("i123e")]
        [InlineData("123:Foo")]
        [InlineData("liel0:ee")]
        public void ReadsStrings(string value)
        {
            var reader = ReaderTests.GetReader($"{Encoding.UTF8.GetByteCount(value)}:{value}");

            Assert.True(reader.Read());
            Assert.Equal(BEncodingTokenType.String, reader.TokenType);
            Assert.True(reader.TryGet(out string readValue));
            Assert.Equal(value, readValue);
        }
    }
}
