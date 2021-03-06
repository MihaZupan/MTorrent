﻿// Copyright (c) Miha Zupan. All rights reserved.
// This file is licensed under the BSD-Clause 2 license.
// See the license.txt file in the project root for more information.
using System;

namespace MTorrent.Helpers
{
    internal static class StringHelpers
    {
        public static bool ContainsAny(this string source, char[] chars)
            => ContainsAny(source.AsSpan(), chars);

        public static bool ContainsAny(this ReadOnlySpan<char> source, char[] chars)
        {
            for (int i = 0; i < chars.Length; i++)
            {
                if (source.Contains(chars[i]))
                    return true;
            }

            return false;
        }

        public static bool LooksLikeValidAnnounceURL(string announce)
        {
            if (announce.Length < 10)
                return false;

            var announceSpan = announce.AsSpan();

            if (!announceSpan.StartsWith("udp://", StringComparison.OrdinalIgnoreCase) &&
                !announceSpan.StartsWith("http://", StringComparison.OrdinalIgnoreCase) &&
                !announceSpan.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
                return false;

            if (!Uri.TryCreate(announce, UriKind.Absolute, out _))
                return false;

            return true;
        }
    }
}
