using System.Linq;
using Xunit;

namespace Torrent.Tests.TorrentFileParsing
{
    public partial class Parsing
    {
        [Theory]
        [InlineData(TestFiles.Ubuntu, new[] { "ubuntu-19.04-desktop-amd64.iso" })]
        [InlineData(TestFiles.Titanic, new[] { "RARBG.txt", "Subs/2_Und.srt", "Titanic.1997.1080p.BluRay.H264.AAC-RARBG.mp4" })]
        [InlineData(TestFiles.Killswitch, new[] { "Killswitch.2014.1080p.WEBRip.x264.AAC.mp4" })]
        [InlineData(TestFiles.InternetsOwnBoy, new[] { "RARBG.txt", "The.Internets.Own.Boy.The.Story.of.Aaron.Swartz.2014.1080p.AMZN.WEB-DL.DDP5.1.H.264-PYRO.mkv" })]
        public void ParsesListOfFiles(string torrentFileName, string[] fileNames)
        {
            TorrentFile torrentFile = ReadAndParseTorrentFile(torrentFileName);

            var files = torrentFile.Files;

            Assert.Equal(fileNames.Length, files.Length);

            Assert.True(files.All(file => file.Length >= 0));
            Assert.True(files.All(file => file.Offset >= 0));
            Assert.True(files.All(file => file.Length + file.Offset >= 0));
            Assert.True(files.All(file => file.Path.Length > 0));
            Assert.True(files.All(file => file.Path.All(part => part.Length > 0)));

            Assert.Equal(torrentFile.TotalBytes, files.Sum(file => file.Length));
            Assert.Equal(0, files[0].Offset);
            Assert.Equal(files[^1].Offset + files[^1].Length, torrentFile.TotalBytes);

            for (int i = 1; i < files.Length; i++)
            {
                long offset = files[i - 1].Offset + files[i - 1].Length;
                Assert.Equal(offset, files[i].Offset);
            }

            Assert.Equal(fileNames, files.Select(file => string.Join('/', file.Path)));
        }
    }
}
