using Torrent.BEncoding;
using Xunit;

namespace Torrent.Tests.BEncoding.Reader
{
    public class Dictionaries
    {
        [Fact]
        public void ReadsDictionaries()
        {
            var reader = ReaderTests.GetReader("d3:foo3:bar3:bari-5ee");

            Assert.True(reader.Read());
            Assert.Equal(BEncodingTokenType.StartDictionary, reader.TokenType);

            Assert.True(reader.Read());
            Assert.Equal(BEncodingTokenType.String, reader.TokenType);
            Assert.True(reader.TryGet(out string key));
            Assert.Equal("foo", key);

            Assert.True(reader.Read());
            Assert.Equal(BEncodingTokenType.String, reader.TokenType);
            Assert.True(reader.TryGet(out string value));
            Assert.Equal("bar", value);

            Assert.True(reader.Read());
            Assert.Equal(BEncodingTokenType.String, reader.TokenType);
            Assert.True(reader.TryGet(out key));
            Assert.Equal("bar", key);

            Assert.True(reader.Read());
            Assert.Equal(BEncodingTokenType.Integer, reader.TokenType);
            Assert.True(reader.TryGet(out int intValue));
            Assert.Equal(-5, intValue);

            Assert.True(reader.Read());
            Assert.Equal(BEncodingTokenType.End, reader.TokenType);
        }

        [Fact]
        public void ReadsEmptyDictionaries()
        {
            var reader = ReaderTests.GetReader("de");

            Assert.True(reader.Read());
            Assert.Equal(BEncodingTokenType.StartDictionary, reader.TokenType);

            Assert.True(reader.Read());
            Assert.Equal(BEncodingTokenType.End, reader.TokenType);
        }

        [Fact]
        public void ReadsNestedDictionaries()
        {
            var reader = ReaderTests.GetReader("dddeee");

            Assert.True(reader.Read());
            Assert.Equal(BEncodingTokenType.StartDictionary, reader.TokenType);

            Assert.True(reader.Read());
            Assert.Equal(BEncodingTokenType.StartDictionary, reader.TokenType);

            Assert.True(reader.Read());
            Assert.Equal(BEncodingTokenType.StartDictionary, reader.TokenType);

            Assert.True(reader.Read());
            Assert.Equal(BEncodingTokenType.End, reader.TokenType);

            Assert.True(reader.Read());
            Assert.Equal(BEncodingTokenType.End, reader.TokenType);

            Assert.True(reader.Read());
            Assert.Equal(BEncodingTokenType.End, reader.TokenType);
        }

        [Fact]
        public void ReadsNestedDictionaryContent()
        {
            var reader = ReaderTests.GetReader("d3:food3:bari3eee");

            Assert.True(reader.Read());
            Assert.Equal(BEncodingTokenType.StartDictionary, reader.TokenType);

            Assert.True(reader.Read());
            Assert.Equal(BEncodingTokenType.String, reader.TokenType);
            Assert.True(reader.TryGet(out string key));
            Assert.Equal("foo", key);

            Assert.True(reader.Read());
            Assert.Equal(BEncodingTokenType.StartDictionary, reader.TokenType);

            Assert.True(reader.Read());
            Assert.Equal(BEncodingTokenType.String, reader.TokenType);
            Assert.True(reader.TryGet(out key));
            Assert.Equal("bar", key);

            Assert.True(reader.Read());
            Assert.Equal(BEncodingTokenType.Integer, reader.TokenType);
            Assert.True(reader.TryGet(out int intValue));
            Assert.Equal(3, intValue);

            Assert.True(reader.Read());
            Assert.Equal(BEncodingTokenType.End, reader.TokenType);

            Assert.True(reader.Read());
            Assert.Equal(BEncodingTokenType.End, reader.TokenType);
        }

        [Fact]
        public void ReadsNestedDictionariesAndLists()
        {
            var reader = ReaderTests.GetReader("d4: foold4:bar leeiee0:0:e");

            Assert.True(reader.Read());
            Assert.Equal(BEncodingTokenType.StartDictionary, reader.TokenType);

            Assert.True(reader.Read());
            Assert.Equal(BEncodingTokenType.String, reader.TokenType);
            Assert.True(reader.TryGet(out string key));
            Assert.Equal(" foo", key);

            Assert.True(reader.Read());
            Assert.Equal(BEncodingTokenType.StartList, reader.TokenType);

            Assert.True(reader.Read());
            Assert.Equal(BEncodingTokenType.StartDictionary, reader.TokenType);

            Assert.True(reader.Read());
            Assert.Equal(BEncodingTokenType.String, reader.TokenType);
            Assert.True(reader.TryGet(out key));
            Assert.Equal("bar ", key);

            Assert.True(reader.Read());
            Assert.Equal(BEncodingTokenType.StartList, reader.TokenType);

            Assert.True(reader.Read());
            Assert.Equal(BEncodingTokenType.End, reader.TokenType);

            Assert.True(reader.Read());
            Assert.Equal(BEncodingTokenType.End, reader.TokenType);

            Assert.True(reader.Read());
            Assert.Equal(BEncodingTokenType.Integer, reader.TokenType);
            Assert.True(reader.TryGet(out int intValue));
            Assert.Equal(0, intValue);

            Assert.True(reader.Read());
            Assert.Equal(BEncodingTokenType.End, reader.TokenType);

            Assert.True(reader.Read());
            Assert.Equal(BEncodingTokenType.String, reader.TokenType);
            Assert.True(reader.TryGet(out key));
            Assert.Equal("", key);

            Assert.True(reader.Read());
            Assert.Equal(BEncodingTokenType.String, reader.TokenType);
            Assert.True(reader.TryGet(out string value));
            Assert.Equal("", value);

            Assert.True(reader.Read());
            Assert.Equal(BEncodingTokenType.End, reader.TokenType);
        }
    }
}
