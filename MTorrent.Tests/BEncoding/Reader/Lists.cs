using Torrent.BEncoding;
using Xunit;

namespace Torrent.Tests.BEncoding.Reader
{
    public class Lists
    {
        [Fact]
        public void ReadsList()
        {
            var reader = ReaderTests.GetReader("li1ei-2ee");

            Assert.True(reader.Read());
            Assert.Equal(BEncodingTokenType.StartList, reader.TokenType);

            Assert.True(reader.Read());
            Assert.Equal(BEncodingTokenType.Integer, reader.TokenType);
            Assert.True(reader.TryGet(out int value));
            Assert.Equal(1, value);

            Assert.True(reader.Read());
            Assert.Equal(BEncodingTokenType.Integer, reader.TokenType);
            Assert.True(reader.TryGet(out value));
            Assert.Equal(-2, value);

            Assert.True(reader.Read());
            Assert.Equal(BEncodingTokenType.End, reader.TokenType);
        }

        [Fact]
        public void ReadsEmptyList()
        {
            var reader = ReaderTests.GetReader("le");

            Assert.True(reader.Read());
            Assert.Equal(BEncodingTokenType.StartList, reader.TokenType);

            Assert.True(reader.Read());
            Assert.Equal(BEncodingTokenType.End, reader.TokenType);
        }

        [Fact]
        public void ReadsNestedLists()
        {
            var reader = ReaderTests.GetReader("llleee");

            Assert.True(reader.Read());
            Assert.Equal(BEncodingTokenType.StartList, reader.TokenType);

            Assert.True(reader.Read());
            Assert.Equal(BEncodingTokenType.StartList, reader.TokenType);

            Assert.True(reader.Read());
            Assert.Equal(BEncodingTokenType.StartList, reader.TokenType);

            Assert.True(reader.Read());
            Assert.Equal(BEncodingTokenType.End, reader.TokenType);

            Assert.True(reader.Read());
            Assert.Equal(BEncodingTokenType.End, reader.TokenType);

            Assert.True(reader.Read());
            Assert.Equal(BEncodingTokenType.End, reader.TokenType);
        }

        [Fact]
        public void ReadsNestedListContent()
        {
            var reader = ReaderTests.GetReader("li1eli2ei22el5:Helloei4e6: worldei5ee");

            Assert.True(reader.Read());
            Assert.Equal(BEncodingTokenType.StartList, reader.TokenType);

            Assert.True(reader.Read());
            Assert.Equal(BEncodingTokenType.Integer, reader.TokenType);
            Assert.True(reader.TryGet(out int value));
            Assert.Equal(1, value);

            Assert.True(reader.Read());
            Assert.Equal(BEncodingTokenType.StartList, reader.TokenType);

            Assert.True(reader.Read());
            Assert.Equal(BEncodingTokenType.Integer, reader.TokenType);
            Assert.True(reader.TryGet(out value));
            Assert.Equal(2, value);

            Assert.True(reader.Read());
            Assert.Equal(BEncodingTokenType.Integer, reader.TokenType);
            Assert.True(reader.TryGet(out value));
            Assert.Equal(22, value);

            Assert.True(reader.Read());
            Assert.Equal(BEncodingTokenType.StartList, reader.TokenType);

            Assert.True(reader.Read());
            Assert.Equal(BEncodingTokenType.String, reader.TokenType);
            Assert.True(reader.TryGet(out string stringValue));
            Assert.Equal("Hello", stringValue);

            Assert.True(reader.Read());
            Assert.Equal(BEncodingTokenType.End, reader.TokenType);

            Assert.True(reader.Read());
            Assert.Equal(BEncodingTokenType.Integer, reader.TokenType);
            Assert.True(reader.TryGet(out value));
            Assert.Equal(4, value);

            Assert.True(reader.Read());
            Assert.Equal(BEncodingTokenType.String, reader.TokenType);
            Assert.True(reader.TryGet(out stringValue));
            Assert.Equal(" world", stringValue);

            Assert.True(reader.Read());
            Assert.Equal(BEncodingTokenType.End, reader.TokenType);

            Assert.True(reader.Read());
            Assert.Equal(BEncodingTokenType.Integer, reader.TokenType);
            Assert.True(reader.TryGet(out value));
            Assert.Equal(5, value);

            Assert.True(reader.Read());
            Assert.Equal(BEncodingTokenType.End, reader.TokenType);
        }
    }
}
