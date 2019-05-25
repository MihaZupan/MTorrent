// Copyright (c) Miha Zupan. All rights reserved.
// This file is licensed under the BSD-Clause 2 license.
// See the license.txt file in the project root for more information.
using Torrent.Enums;
using Xunit;

namespace Torrent.Tests.TorrentFileParsing
{
    public partial class Parsing
    {
        [Theory]
        [InlineData(TestFiles.Ubuntu, true, false)]
        [InlineData(TestFiles.Titanic, true, false)]
        [InlineData(TestFiles.Killswitch, true, true)]
        [InlineData(TestFiles.InternetsOwnBoy, true, false)]
        public void ParsesCorrectVersions(string torrentFileName, bool v1, bool v2)
        {
            TorrentFile torrentFile = ReadAndParseTorrentFile(torrentFileName);

            Assert.Equal(v1, torrentFile.Version.HasFlag(BitTorrentVersion.V1));
            Assert.Equal(v2, torrentFile.Version.HasFlag(BitTorrentVersion.V2));
        }
    }
}
