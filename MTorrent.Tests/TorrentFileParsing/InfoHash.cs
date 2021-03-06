﻿// Copyright (c) Miha Zupan. All rights reserved.
// This file is licensed under the BSD-Clause 2 license.
// See the license.txt file in the project root for more information.
using System;
using MTorrent.Enums;
using Xunit;

namespace MTorrent.Tests.torrentInfoParsing
{
    public partial class Parsing
    {
        [Theory]
        [InlineData(TestFiles.Ubuntu, "D540FC48EB12F2833163EED6421D449DD8F1CE1F")]
        [InlineData(TestFiles.Titanic, "D67D2D8A534D4EDA06651E8CB0C546133F6B4305")]
        [InlineData(TestFiles.Killswitch, "FE3DFA6764C01AB0A8528EF768DDCEDF2CC0915B")]
        [InlineData(TestFiles.Killswitch, "0BBF263641F111C6122A6019319D7573D10FFABFC6981710218C6CEBA775CACF")]
        [InlineData(TestFiles.InternetsOwnBoy, "CBECA3B91C54DB4B7A7E16BC0CCD9707E00665B5")]
        [InlineData(TestFiles.ProjectEuler, "5B47CA743B1B4AE79F06ED01A8793F92DDD4ACC1")]
        public void ParsesToCorrectInfoHash(string torrentInfoName, string infoHashHex)
        {
            TorrentInfo torrentInfo = ReadAndParsetorrentInfo(torrentInfoName);

            Assert.Contains(infoHashHex.Length, new[] { 40, 64 });

            var infoHash = infoHashHex.Length == 40 ? torrentInfo.InfoHashV1 : torrentInfo.InfoHashV2;
            Assert.True(infoHash != null, "No valid InfoHash found");
            Assert.Equal(infoHashHex.Length / 2, infoHash.Length);

            Assert.True(torrentInfo.Version.HasFlag(infoHash.Length == 20 ? BitTorrentVersion.V1 : BitTorrentVersion.V2));

            string hex = BitConverter.ToString(infoHash).Replace("-", "");

            Assert.Equal(infoHashHex, hex, StringComparer.OrdinalIgnoreCase);
        }
    }
}
