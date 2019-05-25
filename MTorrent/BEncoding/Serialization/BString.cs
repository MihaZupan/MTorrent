// Copyright (c) Miha Zupan. All rights reserved.
// This file is licensed under the BSD-Clause 2 license.
// See the license.txt file in the project root for more information.
using System;
using System.Diagnostics;
using System.Text;

namespace MTorrent.BEncoding.Serialization
{
    [DebuggerDisplay("{String ?? \"BINARY\"}")]
    public class BString : IBObject
    {
        public int SpanStart { get; set; }
        public int SpanEnd { get; set; }

        public readonly byte[] Binary;

        public readonly string String;

        private static readonly UTF8Encoding _utf8Encoding = new UTF8Encoding(false, throwOnInvalidBytes: true);

        public bool IsString => !(String is null);

        public BString(ReadOnlySpan<byte> data)
        {
            Binary = data.ToArray();
            try
            {
                String = _utf8Encoding.GetString(data);
            }
            catch { }
        }

        public BString(string value)
        {
            String = value;
            Binary = _utf8Encoding.GetBytes(value);
        }
    }
}
