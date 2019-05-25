using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Torrent.BEncoding.Serialization
{
    [DebuggerDisplay("Count: {Count}")]
    public class BDictionary : List<KeyValuePair<BString, IBObject>>, IBObject
    {
        public int SpanStart { get; set; }
        public int SpanEnd { get; set; }

        public bool TryGet(string key, out BDictionary dictionary)
        {
            TryGet(key, out IBObject bObject);
            dictionary = bObject as BDictionary;
            return !(dictionary is null);
        }
        public bool TryGet(string key, out BList list)
        {
            TryGet(key, out IBObject bObject);
            list = bObject as BList;
            return !(list is null);
        }
        public bool TryGet(string key, out BInteger integer)
        {
            TryGet(key, out IBObject bObject);
            integer = bObject as BInteger;
            return !(integer is null);
        }
        public bool TryGet(string key, out BString bstring)
        {
            TryGet(key, out IBObject bObject);
            bstring = bObject as BString;
            return !(bstring is null);
        }

        public bool TryGet(ReadOnlySpan<char> key, out IBObject bObject)
        {
            for (int i = 0; i < Count; i++)
            {
                if (this[i].Key.String is string itemKey && key.SequenceEqual(itemKey))
                {
                    bObject = this[i].Value;
                    return true;
                }
            }

            bObject = null;
            return false;
        }
    }
}
