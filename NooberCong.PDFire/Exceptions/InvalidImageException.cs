namespace NooberCong.PDFire.Exceptions;

public class InvalidImageException : Exception
{
    public InvalidImageException() : base(
        "Given data is currupted or is not an image. Supported image formats: [bmp, png, jpeg, tiff]")
    {
    }
}