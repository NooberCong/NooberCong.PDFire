namespace NooberCong.PDFire.Bookmark;

public class PDFireBookmark
{
    public string Title { get; set; }
    public int PageNumber { get; set; }
    public List<PDFireBookmark> Children { get; set; } = new();
}