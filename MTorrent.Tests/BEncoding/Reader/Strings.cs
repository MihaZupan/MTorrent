// Copyright (c) Miha Zupan. All rights reserved.
// This file is licensed under the BSD-Clause 2 license.
// See the license.txt file in the project root for more information.
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
