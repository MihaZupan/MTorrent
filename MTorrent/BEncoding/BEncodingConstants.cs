// Copyright (c) Miha Zupan. All rights reserved.
// This file is licensed under the BSD-Clause 2 license.
// See the license.txt file in the project root for more information.
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
