// Copyright (c) Miha Zupan. All rights reserved.
// This file is licensed under the BSD-Clause 2 license.
// See the license.txt file in the project root for more information.
using System;
using System.Buffers;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Torrent.BEncoding;
using Torrent.Enums;
using Torrent.Helpers;

namespace Torrent
{

    public class TorrentDirectory
    {
        public readonly string DirectoryName;
        public readonly List<TorrentDirectory> SubDirs = new List<TorrentDirectory>();
        public readonly List<FileDescriptor> Files = new List<FileDescriptor>();

        public TorrentDirectory(string name)
        {
            DirectoryName = name ?? throw new ArgumentNullException(nameof(name));
        }

        internal long Size;
        internal long FileCount;

        internal void Init(bool sort)
        {
            if (sort)
            {
                SubDirs.Sort((a, b) => a.DirectoryName.CompareTo(b.DirectoryName));
                Files.Sort((a, b) => a.FileName.CompareTo(b.FileName));
            }

            FileCount = Files.Count;
            Size = 0;

            foreach (var subDir in SubDirs)
            {
                subDir.Init(sort);
                Size += subDir.Size;
                FileCount += subDir.FileCount;
            }
            foreach (var file in Files)
            {
                Size += file.Size;
            }
        }

        internal FileDescriptor FindSingleFile()
        {
            return FindSingleFile(this);
        }
        private static FileDescriptor FindSingleFile(TorrentDirectory directory)
        {
            Debug.Assert(directory.FileCount == 1);
            if (directory.Files.Count == 1)
                return directory.Files[0];

            foreach (var subDir in directory.SubDirs)
            {
                if (subDir.FileCount == 1)
                    return FindSingleFile(subDir);
            }

            Debug.Fail("Should already return by now");
            return null;
        }
    }

    public class FileDescriptor
    {
        public string Source;
        public string FileName;

        internal FileInfo FileInfo;
        internal long Size;

        public FileDescriptor(string source, string fileName)
        {
            Source = source ?? throw new ArgumentNullException(nameof(source));
            FileName = fileName ?? throw new ArgumentNullException(nameof(fileName));

            FileInfo = new FileInfo(source);
            if (!FileInfo.Exists)
                throw new FileNotFoundException("Source file could not be found", source);

            Size = FileInfo.Length;
        }
    }

    public class TorrentFileBuilder
    {
        public FileDescriptor File;
        public TorrentDirectory Files;

        public bool IsSingleFile => !(File is null);

        public readonly List<string> Trackers = new List<string>();

        public string Name;

        private PieceLength _pieceLength = PieceLength.Auto;
        public PieceLength PieceLength
        {
            get => _pieceLength;
            set
            {
                if (!value.IsValid())
                    throw new ArgumentOutOfRangeException(nameof(value));

                _pieceLength = value;
            }
        }

        private bool IsV1 = true, IsV2 = true;
        public bool DualVersion => _version == BitTorrentVersion.DualVersion;
        private BitTorrentVersion _version = BitTorrentVersion.DualVersion;
        public BitTorrentVersion Version
        {
            get => _version;
            set
            {
                if (!value.IsValid())
                    throw new ArgumentOutOfRangeException(nameof(value));

                _version = value;
                IsV1 = value.HasFlag(BitTorrentVersion.V1);
                IsV2 = value.HasFlag(BitTorrentVersion.V2);
            }
        }

        public Action<long, long> ProgressCallback = null;

        public TorrentFileBuilder(string name, TorrentDirectory directory)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));
            Files = directory ?? throw new ArgumentNullException(nameof(directory));
        }
        public TorrentFileBuilder(FileDescriptor file)
        {
            File = file ?? throw new ArgumentNullException(nameof(file));
            Name = file.FileName;
        }

        public Task BuildAsync(Stream destination)
        {
            if (destination is null)
                throw new ArgumentNullException(nameof(destination));

            return BuildAsync(new Utf8BEncodingWriter(destination));
        }
        public Task BuildAsync(IBufferWriter<byte> bufferWriter)
        {
            if (bufferWriter is null)
                throw new ArgumentNullException(nameof(bufferWriter));

            return BuildAsync(new Utf8BEncodingWriter(bufferWriter));
        }
        private async Task BuildAsync(Utf8BEncodingWriter writer)
        {
            if (File is null && (Files is null || Files.FileCount == 0))
                throw new InvalidOperationException("There has to be at least one file available");

            if (IsSingleFile)
            {
                Files = new TorrentDirectory("root");
                Files.Files.Add(File);
            }

            Files.Init(sort: IsV2);

            if (!IsSingleFile && Files.FileCount == 1)
            {
                File = Files.FindSingleFile();
                Name = File.FileName;
            }

            if (PieceLength == PieceLength.Auto)
                PieceLength = PieceLengthHelper.DecideOptimal(Files, IsV2);

            ProgressCallback?.Invoke(0, 0);

            writer.WriteDictionaryStart();

            WriteTrackers(writer);

            long pieceCount = PieceLengthHelper.PieceCount(Files, IsV2, (int)PieceLength);

            byte[] pieces = new byte[pieceCount * 20];

            writer.WriteASCIIUnchecked("info");
            writer.WriteDictionaryStart();

            /*
                file tree (V2)
                files (V1 && FileCount > 1)
                length (V1 && SingleFile)
                meta version (V2)
                name
                piece length
                pieces (V1)
            */

            if (IsV2)
            {
                writer.WriteASCIIUnchecked("file tree");
                writer.WriteDictionaryStart();
                if (IsSingleFile)
                {
                    writer.Write(File.FileName);
                    writer.WriteDictionaryStart();
                    writer.WriteASCIIUnchecked(string.Empty);
                    writer.WriteDictionaryStart();
                    writer.WriteASCIIUnchecked("length");
                    writer.Write(File.Size);
                    if (File.Size > (int)PieceLength)
                    {
                        writer.WriteASCIIUnchecked("pieces root");
                        // ToDo
                        writer.Write("");
                    }
                    writer.WriteEnd(2);
                }
                else
                {
                    // ToDO
                }
                writer.WriteEnd();
            }

            if (IsV1)
            {
                if (IsSingleFile)
                {
                    writer.WriteASCIIUnchecked("length");
                    writer.Write(File.Size);
                }
                else
                {
                    writer.WriteASCIIUnchecked("files");
                    writer.WriteListStart();
                    List<string> dirStack = new List<string>();
                    Stack<TorrentDirectory> subDirStack = new Stack<TorrentDirectory>();
                    subDirStack.Push(Files);
                    while (subDirStack.TryPop(out TorrentDirectory directory))
                    {
                        if (directory is null)
                        {
                            dirStack.RemoveAt(dirStack.Count - 1);
                            continue;
                        }

                        foreach (var file in directory.Files)
                        {
                            writer.WriteDictionaryStart();

                            writer.WriteASCIIUnchecked("length");
                            writer.Write(file.Size);

                            writer.WriteASCIIUnchecked("path");
                            writer.WriteListStart();
                            foreach (var dir in dirStack)
                                writer.Write(dir);
                            writer.Write(file.FileName);

                            writer.WriteEnd(2);
                        }

                        if (directory.SubDirs.Count > 0)
                        {
                            dirStack.Add(directory.DirectoryName);
                            subDirStack.Push(null);
                            foreach (var subDir in directory.SubDirs)
                                subDirStack.Push(subDir);
                        }
                    }
                    writer.WriteEnd();
                }
            }

            if (IsV2)
            {
                writer.WriteASCIIUnchecked("meta version");
                writer.Write(2);
            }

            writer.WriteASCIIUnchecked("name");
            writer.Write(Name);

            writer.WriteASCIIUnchecked("piece length");
            writer.Write((int)_pieceLength);

            if (IsV1)
            {
                writer.WriteASCIIUnchecked("pieces");
                await writer.WriteAsync(pieces).ConfigureAwait(false);
            }

            writer.WriteEnd();

            if (IsV2)
            {
                writer.WriteASCIIUnchecked("piece layers");
                writer.WriteDictionaryStart();
                // Todo
                writer.WriteEnd();
            }

            writer.WriteEnd();

            await writer.FlushAsync().ConfigureAwait(false);
        }

        private void WriteTrackers(Utf8BEncodingWriter writer)
        {
            if (Trackers.Count == 1)
            {
                writer.Write("announce");
                writer.Write(Trackers[0]);
            }
            else if (Trackers.Count > 1)
            {
                writer.Write("announce-list");
                writer.WriteListStart();
                foreach (var tracker in Trackers)
                {
                    writer.WriteListStart();
                    writer.Write(tracker);
                    writer.WriteEnd();
                }
                writer.WriteEnd();
            }
        }
    }
}
