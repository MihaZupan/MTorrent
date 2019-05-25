// Copyright (c) Miha Zupan. All rights reserved.
// This file is licensed under the BSD-Clause 2 license.
// See the license.txt file in the project root for more information.
using System.Collections.Generic;
using System.Diagnostics;

namespace Torrent.BEncoding.Serialization
{
    [DebuggerDisplay("Count: {Count}")]
    public class BList : List<IBObject>, IBObject
    {
        public int SpanStart { get; set; }
        public int SpanEnd { get; set; }
    }
}
