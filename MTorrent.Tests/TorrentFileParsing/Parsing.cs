// Copyright (c) Miha Zupan. All rights reserved.
// This file is licensed under the BSD-Clause 2 license.
// See the license.txt file in the project root for more information.
using System.Collections.Concurrent;
using System.Diagnostics;
using System.IO;
using System.Text;
using Torrent.BEncoding.Serialization;
using Xunit;

namespace Torrent.Tests.TorrentFileParsing
{
    public partial class Parsing
    {
        private static readonly ConcurrentDictionary<string, byte[]> TorrentFiles = new ConcurrentDictionary<string, byte[]>();

        private static byte[] ReadTorrentFile(string torrentFileName)
        {
            if (!TorrentFiles.TryGetValue(torrentFileName, out byte[] torrentBytes))
            {
                string torrentFilePath = $"../../../TestTorrents/{torrentFileName}.torrent";

                Assert.True(File.Exists(torrentFilePath), "Test torrent file could not be found");

                torrentBytes = File.ReadAllBytes(torrentFilePath);

                TorrentFiles.TryAdd(torrentFileName, torrentBytes);
            }

            return torrentBytes;
        }

        private static TorrentFile ReadAndParseTorrentFile(string torrentFileName)
        {
            var torrentBytes = ReadTorrentFile(torrentFileName);

            Assert.True(TorrentFile.TryParse(torrentBytes, out TorrentFile torrentFile, strictComplianceParsing: true), "Failed to parse torrent");

            return torrentFile;
        }

        private static class TestFiles
        {
            public const string
                Ubuntu = "ubuntu-19.04-desktop-amd64.iso",
                Titanic = "Titanic.1997.1080p",
                Killswitch = "Killswitch.2014.1080p.mp4",
                InternetsOwnBoy = "The.Internets.Own.Boy.The.Story.of.Aaron.Swartz.2014.1080p";
        }

        private static void TestTorrent_ValidBEncode(string bencode, bool valid, bool strictlyValid)
        {
            Assert.True(BEncodingSerializer.TryParse(bencode, out BDictionary _), "This test isn't about valid BEncoding");
            TestTorrent(bencode, valid, strictlyValid);
        }

        private static void TestTorrent(string bencode, bool valid, bool strictlyValid)
        {
            Debug.Assert(!strictlyValid || valid, "If strictly valid it should also be non-strictly valid");

            byte[] torrentFileBytes = Encoding.UTF8.GetBytes(bencode);

            Assert.Equal(valid, TorrentFile.TryParse(torrentFileBytes, out _, strictComplianceParsing: false));
            if (valid)
                Assert.Equal(strictlyValid, TorrentFile.TryParse(torrentFileBytes, out _, strictComplianceParsing: true));
        }
    }
}
