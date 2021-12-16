namespace NooberCong.PDFire.Utils;

public static class StreamUtils
{
    public static MemoryStream Copy(this Stream stream)
    {
        stream.Position = 0;
        var cpyStream = new MemoryStream();
        stream.CopyTo(cpyStream);
        return cpyStream;
    }

    public static byte[] ToBytes(this Stream stream)
    {
        stream.Position = 0;
        var cpyStream = new MemoryStream();
        stream.CopyTo(cpyStream);
        return cpyStream.ToArray();
    }
}