// Copyright (c) Miha Zupan. All rights reserved.
// This file is licensed under the BSD-Clause 2 license.
// See the license.txt file in the project root for more information.
namespace Torrent.Enums
{
    public enum PieceLength
    {
        Auto = 0,

        KB_16   = 1 << 14,
        KB_32   = 1 << 15,
        KB_64   = 1 << 16,
        KB_128  = 1 << 17,
        KB_256  = 1 << 18,
        KB_512  = 1 << 19,
        MB_1    = 1 << 20,
        MB_2    = 1 << 21,
        MB_4    = 1 << 22,
        MB_8    = 1 << 23,
        MB_16   = 1 << 24,

        // Experimental
        MB_32   = 1 << 25,
        MB_64   = 1 << 26,

        MIN     = KB_16,
        MAX     = MB_64
    }

    internal static partial class EnumValidator
    {
        public static bool IsValid(this PieceLength pieceLength)
        {
            if (pieceLength == PieceLength.Auto)
                return true;

            int length = (int)pieceLength;

            if (length > (int)PieceLength.MAX)
                return false;

            while (true)
            {
                if (length < (int)PieceLength.MIN)
                    return false;

                if (length == (int)PieceLength.MIN)
                    return true;

                if ((length & 1) == 1)
                    return false;

                length >>= 1;
            }
        }
    }
}
