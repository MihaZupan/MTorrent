// Copyright (c) Miha Zupan. All rights reserved.
// This file is licensed under the BSD-Clause 2 license.
// See the license.txt file in the project root for more information.
using System.Text;
using MTorrent.BEncoding;
using Xunit;

namespace MTorrent.Tests.BEncoding.Reader
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
