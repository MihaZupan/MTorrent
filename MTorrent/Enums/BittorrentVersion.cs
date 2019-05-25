// Copyright (c) Miha Zupan. All rights reserved.
// This file is licensed under the BSD-Clause 2 license.
// See the license.txt file in the project root for more information.
using System;

namespace Torrent.Enums
{
    [Flags]
    public enum BitTorrentVersion
    {
        V1 = 1,
        V2 = 2,

        DualVersion = V1 | V2
    }

    internal static partial class EnumValidator
    {
        public static bool IsValid(this BitTorrentVersion version)
        {
            return version == BitTorrentVersion.V1 ||
                version == BitTorrentVersion.V2 ||
                version == BitTorrentVersion.DualVersion;
        }
    }
}
