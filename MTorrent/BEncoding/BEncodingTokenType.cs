// Copyright (c) Miha Zupan. All rights reserved.
// This file is licensed under the BSD-Clause 2 license.
// See the license.txt file in the project root for more information.
namespace MTorrent.BEncoding
{
    public enum BEncodingTokenType
    {
        None,
        StartList,
        StartDictionary,
        End,
        Integer,
        String,
    }
}
