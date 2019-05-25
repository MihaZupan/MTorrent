// Copyright (c) Miha Zupan. All rights reserved.
// This file is licensed under the BSD-Clause 2 license.
// See the license.txt file in the project root for more information.
using System.Numerics;
using System.Text;
using Torrent.BEncoding;
using Xunit;

namespace Torrent.Tests.BEncoding.Reader
{
    public class Numbers
    {
        private static Utf8BEncodingReader GetNumberReader(string number)
        {
            string bencoded = 'i' + number + 'e';

            var reader = ReaderTests.GetReader(bencoded);

            Assert.True(reader.Read());
            Assert.Equal(BEncodingTokenType.Integer, reader.TokenType);
            Assert.Equal(number, Encoding.UTF8.GetString(reader.ValueSpan));

            return reader;
        }

        [Theory]
        [InlineData("abc", false)]
        [InlineData("", true)]
        [InlineData("0", true)]
        [InlineData("-0", false)]
        [InlineData("+0", false)]
        [InlineData("00", false)]
        [InlineData("1", true)]
        [InlineData("01", false)]
        [InlineData("101", true)]
        [InlineData("123", true)]
        [InlineData("+123", false)]
        [InlineData("-", false)]
        [InlineData("-1", true)]
        [InlineData("--1", false)]
        [InlineData("-123", true)]
        [InlineData("-0123", false)]
        [InlineData("2147483647", true)]
        [InlineData("2147483648", false)]
        [InlineData("-2147483648", true)]
        [InlineData("-2147483649", false)]
        public void ParsesInt32(string number, bool valid)
        {
            var reader = GetNumberReader(number);
            Assert.Equal(valid, reader.TryGet(out int value));
            if (valid) Assert.Equal(number.Length == 0 ? "0" : number, value.ToString());
        }

        [Theory]
        [InlineData("abc", false)]
        [InlineData("", true)]
        [InlineData("0", true)]
        [InlineData("-0", false)]
        [InlineData("+0", false)]
        [InlineData("00", false)]
        [InlineData("1", true)]
        [InlineData("01", false)]
        [InlineData("101", true)]
        [InlineData("123", true)]
        [InlineData("+123", false)]
        [InlineData("-", false)]
        [InlineData("-1", false)]
        [InlineData("--1", false)]
        [InlineData("-123", false)]
        [InlineData("2147483647", true)]
        [InlineData("2147483648", true)]
        [InlineData("4294967295", true)]
        [InlineData("4294967296", false)]
        public void ParsesUInt32(string number, bool valid)
        {
            var reader = GetNumberReader(number);
            Assert.Equal(valid, reader.TryGet(out uint value));
            if (valid) Assert.Equal(number.Length == 0 ? "0" : number, value.ToString());
        }

        [Theory]
        [InlineData("abc", false)]
        [InlineData("", true)]
        [InlineData("0", true)]
        [InlineData("-0", false)]
        [InlineData("+0", false)]
        [InlineData("00", false)]
        [InlineData("1", true)]
        [InlineData("01", false)]
        [InlineData("101", true)]
        [InlineData("123", true)]
        [InlineData("+123", false)]
        [InlineData("-", false)]
        [InlineData("-1", true)]
        [InlineData("--1", false)]
        [InlineData("-123", true)]
        [InlineData("-0123", false)]
        [InlineData("2147483647", true)]
        [InlineData("2147483648", true)]
        [InlineData("-2147483648", true)]
        [InlineData("-2147483649", true)]
        [InlineData("9223372036854775807", true)]
        [InlineData("9223372036854775808", false)]
        [InlineData("-9223372036854775808", true)]
        [InlineData("-9223372036854775809", false)]
        public void ParsesInt64(string number, bool valid)
        {
            var reader = GetNumberReader(number);
            Assert.Equal(valid, reader.TryGet(out long value));
            if (valid) Assert.Equal(number.Length == 0 ? "0" : number, value.ToString());
        }

        [Theory]
        [InlineData("abc", false)]
        [InlineData("", true)]
        [InlineData("0", true)]
        [InlineData("-0", false)]
        [InlineData("+0", false)]
        [InlineData("00", false)]
        [InlineData("1", true)]
        [InlineData("01", false)]
        [InlineData("101", true)]
        [InlineData("123", true)]
        [InlineData("+123", false)]
        [InlineData("-", false)]
        [InlineData("-1", false)]
        [InlineData("--1", false)]
        [InlineData("-123", false)]
        [InlineData("2147483647", true)]
        [InlineData("2147483648", true)]
        [InlineData("4294967295", true)]
        [InlineData("4294967296", true)]
        [InlineData("9223372036854775807", true)]
        [InlineData("9223372036854775808", true)]
        [InlineData("18446744073709551615", true)]
        [InlineData("18446744073709551616", false)]
        public void ParsesUInt64(string number, bool valid)
        {
            var reader = GetNumberReader(number);
            Assert.Equal(valid, reader.TryGet(out ulong value));
            if (valid) Assert.Equal(number.Length == 0 ? "0" : number, value.ToString());
        }

        [Theory]
        [InlineData("abc", false)]
        [InlineData("", true)]
        [InlineData("0", true)]
        [InlineData("-0", false)]
        [InlineData("+0", false)]
        [InlineData("00", false)]
        [InlineData("1", true)]
        [InlineData("01", false)]
        [InlineData("101", true)]
        [InlineData("123", true)]
        [InlineData("+123", false)]
        [InlineData("-", false)]
        [InlineData("-1", true)]
        [InlineData("--1", false)]
        [InlineData("-123", true)]
        [InlineData("-0123", false)]
        [InlineData("2147483647", true)]
        [InlineData("2147483648", true)]
        [InlineData("-2147483648", true)]
        [InlineData("-2147483649", true)]
        [InlineData("9223372036854775807", true)]
        [InlineData("9223372036854775808", true)]
        [InlineData("-9223372036854775808", true)]
        [InlineData("-9223372036854775809", true)]
        [InlineData("18446744073709551615", true)]
        [InlineData("18446744073709551616", true)]
        [InlineData("123456789012345678901234567890", true)]
        [InlineData("0123456789012345678901234567890", false)]
        [InlineData("54321678905432167890543216789054321678905432167890", true)]
        [InlineData("0054321678905432167890543216789054321678905432167890", false)]
        [InlineData("12345678910111213141516171819202122232425262728293031323334353637383940", true)]
        public void ParsesBigInteger(string number, bool valid)
        {
            var reader = GetNumberReader(number);
            Assert.Equal(valid, reader.TryGet(out BigInteger value));
            if (valid) Assert.Equal(number.Length == 0 ? "0" : number, value.ToString());
        }
    }
}
