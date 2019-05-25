using System;
using System.Diagnostics;

namespace Torrent.BEncoding.Serialization
{
    public static partial class BEncodingSerializer
    {
        public static bool TryParse(string data, out BDictionary dictionary, bool strictDictionaryOrder = true)
            => TryParse(BEncodingHelpers.UTF8.GetBytes(data), out dictionary, strictDictionaryOrder);

        public static bool TryParse(ReadOnlySpan<byte> data, out BDictionary dictionary, bool strictDictionaryOrder = true)
        {
			if (data.Length > 1 && data[0] == BEncodingConstants.OpenDictionary && data[data.Length - 1] == BEncodingConstants.End)
            {
				if (TryParse(data, out IBObject bObject, strictDictionaryOrder))
                {
                    dictionary = bObject as BDictionary;
                    Debug.Assert(dictionary != null);
                    return true;
                }
            }

            dictionary = null;
            return false;
        }
    }
}
