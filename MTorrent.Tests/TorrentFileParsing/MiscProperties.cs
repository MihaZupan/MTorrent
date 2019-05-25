// Copyright (c) Miha Zupan. All rights reserved.
// This file is licensed under the BSD-Clause 2 license.
// See the license.txt file in the project root for more information.
using System;
using Xunit;

namespace MTorrent.Tests.torrentInfoParsing
{
    public partial class Parsing
    {
        [Fact]
        public void ParsesComment()
        {
            // 18 because ö is two UTF8 bytes
            string bencode = "d7:comment18:Some UTF8 cömment4:infod6:lengthi0e4:name3:foo12:piece lengthi16384e6:pieces0:ee";

            var torrentInfo = ParsetorrentInfo(bencode);

            Assert.Equal("Some UTF8 cömment", torrentInfo.Comment);
        }

        [Fact]
        public void ParsesCommentFromFile()
        {
            TorrentInfo torrentInfo = ReadAndParsetorrentInfo(TestFiles.ProjectEuler);

            Assert.Equal("This is some Project Euler", torrentInfo.Comment);
        }

        [Fact]
        public void ParsesCreatedBy()
        {
            string bencode = "d10:created by7:Someone4:infod6:lengthi0e4:name3:foo12:piece lengthi16384e6:pieces0:ee";

            var torrentInfo = ParsetorrentInfo(bencode);

            Assert.Equal("Someone", torrentInfo.CreatedBy);
        }

        [Fact]
        public void ParsesCreatedByFromFile()
        {
            TorrentInfo torrentInfo = ReadAndParsetorrentInfo(TestFiles.ProjectEuler);

            Assert.Equal("uTorrent/3.5.5", torrentInfo.CreatedBy);
        }

        [Fact]
        public void ParsesCreationDate()
        {
            string bencode = "d13:creation datei330e4:infod6:lengthi0e4:name3:foo12:piece lengthi16384e6:pieces0:ee";

            var torrentInfo = ParsetorrentInfo(bencode);

            Assert.Equal(new DateTime(1970, 1, 1, 0, 5, 30), torrentInfo.CreationDate);
        }

        [Fact]
        public void ParsesCreationDateFromFile()
        {
            TorrentInfo torrentInfo = ReadAndParsetorrentInfo(TestFiles.ProjectEuler);

            Assert.Equal(1558810233, torrentInfo.CreationTimeStamp);
            Assert.Equal(new DateTime(2019, 5, 25, 18, 50, 33), torrentInfo.CreationDate);
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
            var torrentInfo = ParsetorrentInfo(bencode);

            Assert.Equal(announceList, torrentInfo.Trackers);
        }

        [Fact]
        public void ParsesAnnounceListFromFile()
        {
            TorrentInfo torrentInfo = ReadAndParsetorrentInfo(TestFiles.ProjectEuler);

            Assert.Equal(new[] { "udp://tracker.mihazupan.me" }, torrentInfo.Trackers);
        }

        [Theory]
        [InlineData("d4:infod6:lengthi0e4:name3:foo12:piece lengthi16384e6:pieces0:ee", null)]
        [InlineData("d8:encoding5:UTF-84:infod6:lengthi0e4:name3:foo12:piece lengthi16384e6:pieces0:ee", "UTF-8")]
        [InlineData("d8:encoding0:4:infod6:lengthi0e4:name3:foo12:piece lengthi16384e6:pieces0:ee", "")]
        public void ParsesEncoding(string bencode, string encoding)
        {
            var torrentInfo = ParsetorrentInfo(bencode);

            Assert.Equal(encoding, torrentInfo.Encoding);
        }

        [Fact]
        public void ParsesEncodingFromFile()
        {
            TorrentInfo torrentInfo = ReadAndParsetorrentInfo(TestFiles.ProjectEuler);

            Assert.Equal("UTF-8", torrentInfo.Encoding);
        }
    }
}
