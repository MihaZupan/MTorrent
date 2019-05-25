// Copyright (c) Miha Zupan. All rights reserved.
// This file is licensed under the BSD-Clause 2 license.
// See the license.txt file in the project root for more information.
namespace MTorrent
{
    internal static class Constants
    {
        public const int MaxFileDirectoryDepth = 16;

        public const uint MaxFilePathLength = 160;

        public static readonly char[] ForbiddenPathPartCharacters = new[] { '/', '\\', '~' };
    }
}
