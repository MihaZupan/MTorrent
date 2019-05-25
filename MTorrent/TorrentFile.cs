// Copyright (c) Miha Zupan. All rights reserved.
// This file is licensed under the BSD-Clause 2 license.
// See the license.txt file in the project root for more information.
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Security.Cryptography;
using Torrent.BEncoding.Serialization;
using Torrent.Enums;
using Torrent.Helpers;

namespace Torrent
{
    internal class TorrentDirectoryDescriptor
    {
        public string Name;
        public List<TorrentDirectoryDescriptor> SubDirs = new List<TorrentDirectoryDescriptor>();
        public List<TorrentFileDescriptor> Files = new List<TorrentFileDescriptor>();

        public TorrentDirectoryDescriptor(string name)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));
        }
    }
    public class TorrentFileDescriptor
    {
        public readonly string[] Path;
        public readonly long Length;
        public readonly long Offset;

        public TorrentFileDescriptor(string path, long length, long offset)
            : this(new[] { path }, length, offset)
        { }
        public TorrentFileDescriptor(string[] path, long length, long offset)
        {
            Path = path ?? throw new ArgumentNullException(nameof(path));
            for (int i = 0; i < Path.Length; i++)
                if (Path[i] is null)
                    throw new ArgumentNullException("Path parts must not be null");

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

    public class TorrentFile
    {
        public BitTorrentVersion Version { get; private set; }
        public byte[] InfoHashV1 { get; private set; }
        public byte[] InfoHashV2 { get; private set; }
        public string DisplayName;
        public bool IsPrivate;
        public PieceLength PieceLength { get; private set; }
        public List<string> Trackers = new List<string>();

        private byte[] _pieces;
        public TorrentFileDescriptor[] Files { get; private set; }
        public long TotalBytes { get; private set; }
        public int FileCount => Files.Length;

        public string Comment = string.Empty;
        public string CreatedBy = string.Empty;
        public long CreationTimeStamp = -1;
        public DateTime? CreationDate = null;

        internal TorrentFile(byte[] infoHashV1, byte[] infoHashV2)
        {
            if (infoHashV1 is null && infoHashV2 is null)
                throw new ArgumentException("At least one InfoHash is needed for a magnet link");

            InfoHashV1 = infoHashV1;
            InfoHashV2 = infoHashV2;
        }
        internal TorrentFile()
        {

        }

        public static bool TryParse(ReadOnlySpan<byte> bytes, out TorrentFile torrent, bool strictComplianceParsing = false)
        {
            torrent = null;

            if (!BEncodingSerializer.TryParse(bytes, out BDictionary dictionary, strictDictionaryOrder: strictComplianceParsing))
                return false;

            if (dictionary.Count > 50)
                return false;

            if (!dictionary.TryGet("info", out BDictionary info))
                return false;

            if (info.Count < 3 || info.Count > 50)
                return false;

            torrent = new TorrentFile();

            if (!info.TryGet("name", out BString name) || !name.IsString)
                return false;

            torrent.DisplayName = name.String;

            if (!info.TryGet("piece length", out BInteger bpieceLength) || !bpieceLength.TryAs(out uint pieceLength))
                return false;

            torrent.PieceLength = (PieceLength)pieceLength;
            if (pieceLength == 0 || !torrent.PieceLength.IsValid())
                return false;

            if (info.TryGet("pieces", out BString pieces))
            {
                if (pieces.Binary.Length % 20 != 0)
                    return false;

                torrent._pieces = pieces.Binary;
                torrent.Version |= BitTorrentVersion.V1;

                if (info.TryGet("length", out BInteger bLength))
                {
                    if (!bLength.TryAs(out long length) || length < 0)
                        return false;

                    torrent.Files = new[] { new TorrentFileDescriptor(torrent.DisplayName, length, offset: 0) };
                    torrent.TotalBytes = length;
                }
                else
                {
                    if (!info.TryGet("files", out BList fileList))
                        return false;

                    if (fileList.Count < (strictComplianceParsing ? 2 : 1))
                        return false;

                    var files = new TorrentFileDescriptor[fileList.Count];

                    long offset = 0;

                    for (int i = 0; i < fileList.Count; i++)
                    {
                        if (!(fileList[i] is BDictionary fileEntry))
                            return false;

                        if (!fileEntry.TryGet("length", out BInteger fileBLength) || !fileBLength.TryAs(out long fileLength))
                            return false;

                        if (fileLength < 0 || fileLength + offset < 0)
                            return false;

                        if (!fileEntry.TryGet("path", out BList pathList))
                            return false;

                        if (pathList.Count == 0 || pathList.Count > Constants.MaxFileDirectoryDepth)
                            return false;

                        int totalPathLength = pathList.Count;
                        string[] path = new string[pathList.Count];
                        for (int j = 0; j < pathList.Count; j++)
                        {
                            if (!(pathList[j] is BString pathBPart) || !pathBPart.IsString)
                                return false;

                            string pathPart = pathBPart.String;

                            if (pathPart.Length == 0)
                                return false;

                            if (!PathHelpers.IsSafeFilePathPart(pathPart))
                                return false;

                            totalPathLength += pathPart.Length;

                            if ((uint)totalPathLength > Constants.MaxFilePathLength)
                                return false;

                            path[j] = pathPart;
                        }

                        files[i] = new TorrentFileDescriptor(path, fileLength, offset);

                        offset += fileLength;
                    }

                    torrent.Files = files;
                    torrent.TotalBytes = offset;
                }

                long pieceCount = (torrent.TotalBytes + pieceLength - 1) / pieceLength;

                // Sanity check on the piece count
                if (pieceLength < (int)PieceLength.MB_8 && pieceCount > 250000)
                    return false;

                if (pieceCount * 20 != pieces.Binary.Length)
                    return false;
            }

            if (info.TryGet("meta version", out BInteger version))
            {
                if (version.Value == 2)
                    torrent.Version |= BitTorrentVersion.V2;
                else if (torrent.Version != BitTorrentVersion.V1 || !version.Value.IsOne)
                    return false;
            }
            else if (torrent.Version == default)
                return false;

            if (torrent.Version.HasFlag(BitTorrentVersion.V2))
            {
                if (!info.TryGet("file tree", out BDictionary fileTree))
                    return false;

                // file tree
                //throw new NotImplementedException();
            }

            if (info.TryGet("private", out BInteger isPrivate) && isPrivate.Value.IsOne)
                torrent.IsPrivate = true;

            if (dictionary.TryGet("announce", out BString announce) && announce.IsString)
                if (StringHelpers.LooksLikeValidAnnounceURL(announce.String))
                    torrent.Trackers.Add(announce.String);

            if (dictionary.TryGet("announce-list", out BList announceList))
                foreach (var announceEntry in announceList)
                    if (announceEntry is BList announceStringList)
                        if (announceStringList.Count == 1 && announceStringList[0] is BString announceString)
                            if (announceString.IsString)
                                if (StringHelpers.LooksLikeValidAnnounceURL(announceString.String))
                                    torrent.Trackers.Add(announceString.String);

            if (dictionary.TryGet("comment", out BString comment) && comment.IsString)
                torrent.Comment = comment.String;

            if (dictionary.TryGet("created by", out BString createdBy) && createdBy.IsString)
                torrent.CreatedBy = createdBy.String;

            if (dictionary.TryGet("creation date", out BInteger bcreationDate) && bcreationDate.TryAs(out long creationDate))
            {
                if (creationDate >= 0)
                {
                    torrent.CreationTimeStamp = creationDate;
                    torrent.CreationDate = DateTime.UnixEpoch.AddSeconds(creationDate);
                }
            }


            ReadOnlySpan<byte> infoSpan = bytes.Slice(info.SpanStart, info.SpanEnd - info.SpanStart);
            Debug.Assert(infoSpan[0] == 'd' && infoSpan[infoSpan.Length - 1] == 'e');
            if (torrent.Version.HasFlag(BitTorrentVersion.V1))
            {
                torrent.InfoHashV1 = new byte[20];
                using SHA1 sha1 = SHA1.Create();
                if (!sha1.TryComputeHash(infoSpan, torrent.InfoHashV1, out int written) || written != 20)
                    return false;
            }
            if (torrent.Version.HasFlag(BitTorrentVersion.V2))
            {
                torrent.InfoHashV2 = new byte[32];
                using SHA256 sha256 = SHA256.Create();
                if (!sha256.TryComputeHash(infoSpan, torrent.InfoHashV2, out int written) || written != 32)
                    return false;
            }

            return true;
        }
    }
}
