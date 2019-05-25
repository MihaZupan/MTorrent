using Xunit;

namespace Torrent.Tests.TorrentFileParsing
{
    public partial class Parsing
    {
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
        [InlineData("d4:infod5:filesld6:lengthi0e4:pathl3:foeeee4:name3:foo12:piece lengthi16384e6:pieces0:ee")]
        public void RejectSingleFileFilesFieldUnderStrictParsing(string bencode)
            => TestTorrent_ValidBEncode(bencode, true, false);

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
        // Missing info
        [InlineData("d0:0:e")]
        // Missing name
        [InlineData("d4:infod6:lengthi0e8:notaname3:foo12:piece lengthi16384e6:pieces0:ee")]
        // Missing length/files
        [InlineData("d4:infod4:name3:foo12:piece lengthi16384e6:pieces0:ee")]
        public void RejectsInvalidFiles(string bencode, bool valid = false)
            => TestTorrent_ValidBEncode(bencode, valid, valid);
    }
}
