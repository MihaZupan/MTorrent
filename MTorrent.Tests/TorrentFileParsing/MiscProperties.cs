// Copyright (c) Miha Zupan. All rights reserved.
// This file is licensed under the BSD-Clause 2 license.
// See the license.txt file in the project root for more information.
using System;
using Xunit;

namespace Torrent.Tests.TorrentFileParsing
{
    public partial class Parsing
    {
        [Fact]
        public void ParsesComment()
        {
            // 18 because ö is two UTF8 bytes
            string bencode = "d7:comment18:Some UTF8 cömment4:infod6:lengthi0e4:name3:foo12:piece lengthi16384e6:pieces0:ee";

            var torrentFile = ParseTorrentFile(bencode);

            Assert.Equal("Some UTF8 cömment", torrentFile.Comment);
        }

        [Fact]
        public void ParsesCommentFromFile()
        {
            TorrentFile torrentFile = ReadAndParseTorrentFile(TestFiles.ProjectEuler);

            Assert.Equal("This is some Project Euler", torrentFile.Comment);
        }

        [Fact]
        public void ParsesCreatedBy()
        {
            string bencode = "d10:created by7:Someone4:infod6:lengthi0e4:name3:foo12:piece lengthi16384e6:pieces0:ee";

            var torrentFile = ParseTorrentFile(bencode);

            Assert.Equal("Someone", torrentFile.CreatedBy);
        }

        [Fact]
        public void ParsesCreatedByFromFile()
        {
            TorrentFile torrentFile = ReadAndParseTorrentFile(TestFiles.ProjectEuler);

            Assert.Equal("uTorrent/3.5.5", torrentFile.CreatedBy);
        }

        [Fact]
        public void ParsesCreationDate()
        {
            string bencode = "d13:creation datei330e4:infod6:lengthi0e4:name3:foo12:piece lengthi16384e6:pieces0:ee";

            var torrentFile = ParseTorrentFile(bencode);

            Assert.Equal(new DateTime(1970, 1, 1, 0, 5, 30), torrentFile.CreationDate);
        }

        [Fact]
        public void ParsesCreationDateFromFile()
        {
            TorrentFile torrentFile = ReadAndParseTorrentFile(TestFiles.ProjectEuler);

            Assert.Equal(1558810233, torrentFile.CreationTimeStamp);
            Assert.Equal(new DateTime(2019, 5, 25, 18, 50, 33), torrentFile.CreationDate);
        }

        [Theory]
        [InlineData("d4:infod6:lengthi0e4:name3:foo12:piece lengthi16384e6:pieces0:ee", new string[] { })]
        [InlineData("d8:announce13:udp://foo.bar4:infod6:lengthi0e4:name3:foo12:piece lengthi16384e6:pieces0:ee", new string[] { "udp://foo.bar" })]
        [InlineData("d13:announce-listll13:udp://foo.baree4:infod6:lengthi0e4:name3:foo12:piece lengthi16384e6:pieces0:ee", new string[] { "udp://foo.bar" })]
        // TorrentEngine should filter out duplicates
        [InlineData("d8:announce13:udp://foo.bar13:announce-listll13:udp://foo.baree4:infod6:lengthi0e4:name3:foo12:piece lengthi16384e6:pieces0:ee", new string[] { "udp://foo.bar", "udp://foo.bar" })]
        [InlineData("d13:announce-listll14:http://foo.barel13:udp://foo.baree4:infod6:lengthi0e4:name3:foo12:piece lengthi16384e6:pieces0:ee", new string[] { "http://foo.bar", "udp://foo.bar" })]
        [InlineData("d13:announce-listll24:https://foo.bar/announceel13:udp://foo.baree4:infod6:lengthi0e4:name3:foo12:piece lengthi16384e6:pieces0:ee", new string[] { "https://foo.bar/announce", "udp://foo.bar" })]
        public void ParsesAnnounceLists(string bencode, string[] announceList)
        {
            var torrentFile = ParseTorrentFile(bencode);

            Assert.Equal(announceList, torrentFile.Trackers);
        }

        [Fact]
        public void ParsesAnnounceListFromFile()
        {
            TorrentFile torrentFile = ReadAndParseTorrentFile(TestFiles.ProjectEuler);

            Assert.Equal(new[] { "udp://tracker.mihazupan.me" }, torrentFile.Trackers);
        }
    }
}
