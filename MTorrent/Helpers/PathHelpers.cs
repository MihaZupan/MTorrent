// Copyright (c) Miha Zupan. All rights reserved.
// This file is licensed under the BSD-Clause 2 license.
// See the license.txt file in the project root for more information.
using System;
using System.Collections.Generic;
using System.IO;

namespace Torrent.Helpers
{
    internal static class PathHelpers
    {
        public static string GetCommonRoot(IEnumerable<string> files)
        {
            // ToDo - very optimizable everywhere

            using (var e = files.GetEnumerator())
            {
                if (!e.MoveNext())
                    return string.Empty;

                string root = Path.GetDirectoryName(e.Current).Replace('\\', '/');

                while (e.MoveNext())
                {
                    string dir = e.Current.Replace('\\', '/');

                    if (dir.StartsWith(root, StringComparison.Ordinal))
                        continue;

                    int i = 0;
                    int limit = Math.Min(root.Length, dir.Length);
                    for (; i < limit; i++)
                    {
                        if (dir[i] != root[i])
                            break;
                    }

                    if (i == limit)
                    {
                        if (root.Length != limit)
                            root = dir;

                        continue;
                    }

                    int end = dir.AsSpan(0, i).LastIndexOf('/');

                    root = dir.AsSpan(0, end).ToString();
                }

                return root;
            }
        }

        public static bool IsSafeFilePathPart(ReadOnlySpan<char> path)
        {
            if (path.ContainsAny(Constants.ForbiddenPathPartCharacters))
                return false;

            if (path.Contains("..", StringComparison.Ordinal))
                return false;

            return true;
        }
    }
}
