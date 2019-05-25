// Copyright (c) Miha Zupan. All rights reserved.
// This file is licensed under the BSD-Clause 2 license.
// See the license.txt file in the project root for more information.
using System.Collections.Concurrent;
using System.Diagnostics;
using System.IO;
using System.Text;
using MTorrent.BEncoding.Serialization;
using Xunit;

namespace MTorrent.Tests.torrentInfoParsing
{
    public partial class Parsing
    {
        private static readonly ConcurrentDictionary<string, byte[]> torrentInfos = new ConcurrentDictionary<string, byte[]>();

        private static byte[] ReadtorrentInfo(string torrentInfoName)
        {
            if (!torrentInfos.TryGetValue(torrentInfoName, out byte[] torrentBytes))
            {
                string torrentInfoPath = $"../../../TestTorrents/{torrentInfoName}.torrent";

                Assert.True(System.IO.File.Exists(torrentInfoPath), "Test torrent file could not be found");

                torrentBytes = System.IO.File.ReadAllBytes(torrentInfoPath);

                torrentInfos.TryAdd(torrentInfoName, torrentBytes);
            }

            return torrentBytes;
        }

        private static TorrentInfo ReadAndParsetorrentInfo(string torrentInfoName)
        {
            var torrentBytes = ReadtorrentInfo(torrentInfoName);

            Assert.True(TorrentInfo.TryParse(torrentBytes, out TorrentInfo torrentInfo, strictComplianceParsing: true), "Failed to parse torrent");

            return torrentInfo;
        }
        private static TorrentInfo ParsetorrentInfo(string bencode)
        {
            var torrentBytes = Encoding.UTF8.GetBytes(bencode);

            Assert.True(TorrentInfo.TryParse(torrentBytes, out TorrentInfo torrentInfo, strictComplianceParsing: true), "Failed to parse torrent");

            return torrentInfo;
        }

        private static class TestFiles
        {
            public const string
                Ubuntu = "ubuntu-19.04-desktop-amd64.iso",
                Titanic = "Titanic.1997.1080p",
                Killswitch = "Killswitch.2014.1080p.mp4",
                InternetsOwnBoy = "The.Internets.Own.Boy.The.Story.of.Aaron.Swartz.2014.1080p",
                ProjectEuler = "ProjectEuler";
        }

        private static void TestTorrent_ValidBEncode(string bencode, bool valid, bool strictlyValid)
        {
            Assert.True(BEncodingSerializer.TryParse(bencode, out BDictionary _), "This test isn't about valid BEncoding");
            TestTorrent(bencode, valid, strictlyValid);
        }

        private static void TestTorrent(string bencode, bool valid, bool strictlyValid)
        {
            Debug.Assert(!strictlyValid || valid, "If strictly valid it should also be non-strictly valid");

            byte[] torrentInfoBytes = Encoding.UTF8.GetBytes(bencode);

            Assert.Equal(valid, TorrentInfo.TryParse(torrentInfoBytes, out _, strictComplianceParsing: false));
            if (valid)
                Assert.Equal(strictlyValid, TorrentInfo.TryParse(torrentInfoBytes, out _, strictComplianceParsing: true));
        }
    }
}
