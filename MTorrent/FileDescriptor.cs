// Copyright (c) Miha Zupan. All rights reserved.
// This file is licensed under the BSD-Clause 2 license.
// See the license.txt file in the project root for more information.
using System;

namespace MTorrent
{
    public class FileDescriptor
    {
        public readonly string[] Path;
        public readonly string FullPath;
        public readonly long Length;
        public readonly long Offset;

        public FileDescriptor(string path, long length, long offset)
            : this(new[] { path }, length, offset)
        { }
        public FileDescriptor(string[] path, long length, long offset)
        {
            Path = path ?? throw new ArgumentNullException(nameof(path));
            for (int i = 0; i < Path.Length; i++)
                if (Path[i] is null)
                    throw new ArgumentNullException("Path parts must not be null");

            FullPath = string.Join('/', Path);

            Length = length;
            Offset = offset;

            if (Length < 0)
                throw new ArgumentOutOfRangeException(nameof(length));

            if (Offset < 0)
                throw new ArgumentOutOfRangeException(nameof(offset));

            if (length + offset < 0)
                throw new ArgumentOutOfRangeException("length and offset", "Length and offset overflow");
        }
    }
}
