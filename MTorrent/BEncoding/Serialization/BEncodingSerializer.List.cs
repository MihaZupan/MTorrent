using System;
using System.Diagnostics;

namespace Torrent.BEncoding.Serialization
{
    public static partial class BEncodingSerializer
    {
        public static bool TryParse(ReadOnlySpan<byte> data, out BList list)
        {
            if (data.Length > 1 && data[0] == BEncodingConstants.OpenList && data[data.Length - 1] == BEncodingConstants.End)
            {
                if (TryParse(data, out IBObject bObject))
                {
                    list = bObject as BList;
                    Debug.Assert(list != null);
                    return true;
                }
            }

            list = null;
            return false;
        }
    }
}
