// Copyright (c) Miha Zupan. All rights reserved.
// This file is licensed under the BSD-Clause 2 license.
// See the license.txt file in the project root for more information.
/*
using MTorrent.Enums;

namespace MTorrent.Helpers
{
    internal static class PieceLengthHelper
    {
        public static long PieceCount(TorrentDirectory directory, bool version2, int pieceLength)
        {
            if (version2)
                return PieceCountDfs(directory, pieceLength);
            else
                return (directory.Size + pieceLength - 1) / pieceLength;
        }

        private static long PieceCountDfs(TorrentDirectory directory, int pieceLength)
        {
            long count = 0;
            foreach (var subDir in directory.SubDirs)
                count += PieceCountDfs(subDir, pieceLength);
            foreach (var file in directory.Files)
                count += (file.Size + pieceLength - 1) / pieceLength;
            return count;
        }

        public static PieceLength DecideOptimal(TorrentDirectory directory, bool version2)
        {
            long sizeMB = directory.Size >> 20;

            if (!version2)
            {
                if (sizeMB < 16) // Up to 1000 pieces (20 kB)
                    return PieceLength.KB_16;

                if (sizeMB < 40) // Up to 1250 pieces (25 kB)
                    return PieceLength.KB_32;

                if (sizeMB < 80) // Up to 1250 pieces (25 kB)
                    return PieceLength.KB_64;

                if (sizeMB < 192) // Up to 1500 pieces (30 kB)
                    return PieceLength.KB_128;

                if (sizeMB < 512) // Up to 2000 pieces (40 kB)
                    return PieceLength.KB_256;

                if (sizeMB < 1280) // Up to 2500 pieces (50 kB)
                    return PieceLength.KB_512;

                if (sizeMB < 3072) // Up to 3000 pieces (60 kB)
                    return PieceLength.MB_1;

                if (sizeMB < 8192) // Up to 4000 pieces (80 kB)
                    return PieceLength.MB_2;

                if (sizeMB < 20480) // Up to 5000 pieces (100 kB)
                    return PieceLength.MB_4;

                // < 50 GB
                if (sizeMB < 51200) // Up to 6250 pieces (125 kB)
                    return PieceLength.MB_8;

                // < 128 GB
                if (sizeMB < 131072) // Up to 8000 pieces (160 kB)
                    return PieceLength.MB_16;

                // < 640 GB
                if (sizeMB < 655360) // Up to 20000 pieces (400 kB)
                    return PieceLength.MB_32;

                // >= 640 GB
                return PieceLength.MB_64; // Min 10000 pieces
            }
            else
            {
                long avgFileSizeKB = (directory.Size / directory.FileCount) >> 10;

                // For torrents with small files (up to 1 MB)
                if (avgFileSizeKB < 1024)
                {
                    if (avgFileSizeKB < 16)
                        return PieceLength.KB_16;

                    if (avgFileSizeKB < 32)
                        return PieceLength.KB_32;

                    if (avgFileSizeKB < 64)
                        return PieceLength.KB_64;

                    if (avgFileSizeKB < 128)
                        return PieceLength.KB_128;

                    if (avgFileSizeKB < 256)
                        return PieceLength.KB_256;

                    if (avgFileSizeKB < 512)
                        return PieceLength.KB_512;

                    return PieceLength.MB_1;
                }

                if (sizeMB < 1000)
                {
                    long pieces = PieceCountDfs(directory, (int)PieceLength.KB_128);

                    if (pieces > 1500)
                    {
                        if (PieceCountDfs(directory, (int)PieceLength.KB_256) < 2000)
                            return PieceLength.KB_256;

                        return PieceLength.KB_512;
                    }
                    else if (pieces < 625)
                    {
                        pieces = PieceCountDfs(directory, (int)PieceLength.KB_32);

                        if (pieces > 1250)
                            return PieceLength.KB_64;
                        else if (pieces < 500)
                            return PieceLength.KB_16;
                        else
                            return PieceLength.KB_32;
                    }
                    else return PieceLength.KB_128;
                }
                else
                {
                    long pieces = PieceCountDfs(directory, (int)PieceLength.MB_4);

                    if (pieces > 5000)
                    {
                        pieces = PieceCountDfs(directory, (int)PieceLength.MB_16);

                        if (pieces > 8000)
                        {
                            if (sizeMB < 655360)
                                return PieceLength.MB_32;
                            else
                                return PieceLength.KB_64;
                        }
                        else if (pieces < 3125)
                            return PieceLength.MB_8;
                        else
                            return PieceLength.MB_16;
                    }
                    else if (pieces < 2000)
                    {
                        pieces = PieceCountDfs(directory, (int)PieceLength.MB_1);

                        if (pieces > 3000)
                            return PieceLength.MB_2;
                        else if (pieces < 1250)
                            return PieceLength.KB_512;
                        else
                            return PieceLength.MB_1;
                    }
                    else return PieceLength.MB_4;
                }
            }
        }
    }
}
*/