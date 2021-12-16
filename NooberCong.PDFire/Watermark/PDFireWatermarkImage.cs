using System.Drawing;
using NooberCong.PDFire.Exceptions;
using NooberCong.PDFire.Utils;

namespace NooberCong.PDFire.Watermark;

public class PDFireWatermarkImage : IDisposable
{
    private readonly Stream _imageStream;

    public PDFireWatermarkImage(Image image)
    {
        var imageBytes = image.ToBytes();
        if (!imageBytes.IsImage())
        {
            throw new InvalidImageException();
        }

        _imageStream = new MemoryStream(imageBytes);
    }

    public PDFireWatermarkImage(Stream imageStream)
    {
        var imageBytes = imageStream.ToBytes();
        if (!imageBytes.IsImage())
        {
            throw new InvalidImageException();
        }

        _imageStream = new MemoryStream(imageBytes);
    }

    public PDFireWatermarkImage(string filePath)
    {
        var imageBytes = File.ReadAllBytes(filePath);
        if (!imageBytes.IsImage())
        {
            throw new InvalidImageException();
        }

        _imageStream = new MemoryStream(imageBytes);
    }

    public PDFireWatermarkImage(Uri imageUri)
    {
        var imageBytes = imageUri.DownloadData();
        if (!imageBytes.IsImage())
        {
            throw new InvalidImageException();
        }

        _imageStream = new MemoryStream(imageBytes);
    }

    public Image GetImage()
    {
        return Image.FromStream(_imageStream);
    }

    public void Dispose()
    {
        _imageStream.Dispose();
    }
}