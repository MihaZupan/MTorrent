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
