namespace Torrent.BEncoding.Serialization
{
    public interface IBObject
    {
        int SpanStart { get; set; }
        int SpanEnd { get; set; }
    }
}
