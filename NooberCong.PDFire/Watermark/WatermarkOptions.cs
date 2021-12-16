namespace NooberCong.PDFire.Watermark;

public class WatermarkOptions
{
    /// <summary>
    /// Watermark rotation in terms of degree, valid value range from -360 to 360
    /// </summary>
    public float RotationDegree { get; set; }

    /// <summary>
    /// How visible the watermark will be, valid value range from >0 to 1
    /// </summary>
    public float Opacity { get; }

    /// <summary>
    /// Ratio between watermark width and page width, valid value range from >0
    /// </summary>
    public float WidthRelativeToPageWidth { get; set; }

    /// <summary>
    /// Where the watermark will be placed on the page
    /// </summary>
    public WatermarkPosition Position { get; set; }

    public WatermarkOptions(float rotationDegree = -45, float opacity = 0.125f, float widthRelativeToPageWidth = 0.75f,
        WatermarkPosition position = WatermarkPosition.Center)
    {
        RotationDegree = rotationDegree;
        Opacity = opacity;
        WidthRelativeToPageWidth = widthRelativeToPageWidth;
        Position = position;
        Validate();
    }

    private void Validate()
    {
        if (RotationDegree is < -360 or > 360)
        {
            throw new ArgumentOutOfRangeException(nameof(RotationDegree), "Must be in range [-360, 360]");
        }

        if (Opacity is <= 0 or > 1)
        {
            throw new ArgumentOutOfRangeException(nameof(Opacity), "Must be in range (0, 1]");
        }

        if (WidthRelativeToPageWidth <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(WidthRelativeToPageWidth), "must be greater than 0");
        }
    }
}