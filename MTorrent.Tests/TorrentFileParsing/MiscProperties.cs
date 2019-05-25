// Copyright (c) Miha Zupan. All rights reserved.
// This file is licensed under the BSD-Clause 2 license.
// See the license.txt file in the project root for more information.
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
        public void ParsesCreatedBy()
        {
            string bencode = "d10:created by7:Someone4:infod6:lengthi0e4:name3:foo12:piece lengthi16384e6:pieces0:ee";

            var torrentFile = ParseTorrentFile(bencode);

            Assert.Equal("Someone", torrentFile.CreatedBy);
        }

        [Fact]
        public void ParsesCreationDate()
        {
            string bencode = "d13:creation datei123123e4:infod6:lengthi0e4:name3:foo12:piece lengthi16384e6:pieces0:ee";

            var torrentFile = ParseTorrentFile(bencode);

            Assert.Equal(123123, torrentFile.CreationDate);
        }
    }
}
