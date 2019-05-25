// Copyright (c) Miha Zupan. All rights reserved.
// This file is licensed under the BSD-Clause 2 license.
// See the license.txt file in the project root for more information.
using MTorrent.Enums;
using Xunit;

namespace MTorrent.Tests.torrentInfoParsing
{
    public partial class Parsing
    {
        [Theory]
        [InlineData(TestFiles.Ubuntu, true, false)]
        [InlineData(TestFiles.Titanic, true, false)]
        [InlineData(TestFiles.Killswitch, true, true)]
        [InlineData(TestFiles.InternetsOwnBoy, true, false)]
        [InlineData(TestFiles.ProjectEuler, true, false)]
        public void ParsesCorrectVersions(string torrentInfoName, bool v1, bool v2)
        {
            TorrentInfo torrentInfo = ReadAndParsetorrentInfo(torrentInfoName);

            Assert.Equal(v1, torrentInfo.Version.HasFlag(BitTorrentVersion.V1));
            Assert.Equal(v2, torrentInfo.Version.HasFlag(BitTorrentVersion.V2));
        }
    }
}
