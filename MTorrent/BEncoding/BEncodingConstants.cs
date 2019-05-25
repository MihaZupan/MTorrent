namespace Torrent.BEncoding
{
    internal static class BEncodingConstants
    {
        public const byte OpenInteger = (byte)'i';
        public const byte OpenList = (byte)'l';
        public const byte OpenDictionary = (byte)'d';
        public const byte End = (byte)'e';
        public const byte ColonSeparator = (byte)':';

        public const byte NegativeSign = (byte)'-';

        public const int DepthMax = 64;

        public const int MaxExpansionFactorWhileTranscoding = 3;
    }
}
