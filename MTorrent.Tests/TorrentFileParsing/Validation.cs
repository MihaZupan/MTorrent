using System.Diagnostics;
using System.Text;
using Torrent.BEncoding.Serialization;
using Xunit;

namespace Torrent.Tests.TorrentFileParsing
{
    public partial class Parsing
    {
        private static void TestTorrent(string bencode, bool valid, bool strictlyValid)
        {
            Debug.Assert(!strictlyValid || valid, "If strictly valid it should also be non-strictly valid");

            byte[] torrentFileBytes = Encoding.UTF8.GetBytes(bencode);

            Assert.Equal(valid, TorrentFile.TryParse(torrentFileBytes, out _, strictComplianceParsing: false));
            if (valid)
                Assert.Equal(strictlyValid, TorrentFile.TryParse(torrentFileBytes, out _, strictComplianceParsing: true));
        }

        [Theory]
        [InlineData("foo")]
        [InlineData("d4:name3:foo12:piece lengthi16384e4:infod6:pieces0:6:lengthi0ee")]
        [InlineData("d4:name3:foo12:piece lengthi16384e4:infod6:pieces0:6:lengthi0eeee")]
        public void RejectsInvalidBEncoding(string bencode)
            => TestTorrent(bencode, false, false);

        [Theory]
        [InlineData("d4:infod6:lengthi0e4:name3:foo12:piece lengthi16384e6:pieces0:ee", true)]
        [InlineData("d4:infod4:name3:foo6:pieces0:6:lengthi0e12:piece lengthi16384eee", false)]
        public void RejectsInvalidDictionaryOrderUnderStrictParsing(string bencode, bool strictlyValid)
            => TestTorrent(bencode, true, strictlyValid);

        [Theory]
        // Valid
        [InlineData("d4:infod6:lengthi0e4:name3:foo12:piece lengthi16384e6:pieces0:ee", true)]
        // Negative length
        [InlineData("d4:infod6:lengthi-1e4:name3:foo12:piece lengthi16384e6:pieces0:ee")]
        // Length but no pieces
        [InlineData("d4:infod6:lengthi1e4:name3:foo12:piece lengthi16384e6:pieces0:ee")]
        // Invalid pieces length (mod 20)
        [InlineData("d4:infod6:lengthi0e4:name3:foo12:piece lengthi16384e6:pieces10:aaaaaaaaaaee")]
        // Invalid pieces length for length
        [InlineData("d4:infod6:lengthi16385e4:name3:foo12:piece lengthi16384e6:pieces20:aaaaaaaaaabbbbbbbbbbee")]
        public void RejectsInvalidFiles(string bencode, bool valid = false)
        {
            Assert.True(BEncodingSerializer.TryParse(bencode, out BDictionary _), "This test isn't about valid BEncoding");
            TestTorrent(bencode, valid, valid);
        }
    }
}
