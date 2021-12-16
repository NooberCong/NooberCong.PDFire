using PDFiumSharp.Types;

namespace NooberCong.PDFire.Rendering;

public class PDFireColor
{
    internal readonly FPDF_COLOR _color;

    public PDFireColor(byte red, byte green, byte blue, byte alpha)
    {
        _color = new FPDF_COLOR(red, green, blue, alpha);
    }

    public PDFireColor(byte red, byte green, byte blue)
    {
        _color = new FPDF_COLOR(red, green, blue, 255);
    }

    public static PDFireColor White()
    {
        return new PDFireColor(255, 255, 255);
    }
}