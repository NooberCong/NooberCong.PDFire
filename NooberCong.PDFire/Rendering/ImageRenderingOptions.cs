namespace NooberCong.PDFire.Rendering;

public class ImageRenderingOptions
{
    public float Width { get; set; }
    public ImageQuality Quality { get; set; }
    public PDFireColor BackgroundColor { get; set; }

    public ImageRenderingOptions(float width = 1100, ImageQuality quality = ImageQuality.Medium,
        PDFireColor? backgroundColor = null)
    {
        Width = width;
        Quality = quality;
        BackgroundColor = backgroundColor ?? PDFireColor.White();

        Validate();
    }

    private void Validate()
    {
        if (Width <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(Width), "Must be greater than 0");
        }
    }
}