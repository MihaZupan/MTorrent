using System;
using System.Collections.Generic;
using System.Text;

namespace Torrent
{
    internal static class Constants
    {
        public const int MaxFileDirectoryDepth = 16;

        public const uint MaxFilePathLength = 160;

        public static readonly char[] ForbiddenPathPartCharacters = new[] { '/', '\\', '~' };
    }
}
