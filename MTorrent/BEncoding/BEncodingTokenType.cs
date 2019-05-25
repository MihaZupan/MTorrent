namespace Torrent.BEncoding
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
