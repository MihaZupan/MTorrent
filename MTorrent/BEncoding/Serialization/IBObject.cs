// Copyright (c) Miha Zupan. All rights reserved.
// This file is licensed under the BSD-Clause 2 license.
// See the license.txt file in the project root for more information.
namespace MTorrent.BEncoding.Serialization
{
    public interface IBObject
    {
        int SpanStart { get; set; }
        int SpanEnd { get; set; }
    }
}
