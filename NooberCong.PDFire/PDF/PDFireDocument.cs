using System.Drawing;
using iText.IO.Image;
using iText.Kernel.Pdf;
using iText.Kernel.Pdf.Canvas;
using iText.Kernel.Pdf.Extgstate;
using NooberCong.PDFire.Bookmark;
using NooberCong.PDFire.Exceptions;
using NooberCong.PDFire.Rendering;
using NooberCong.PDFire.Utils;
using NooberCong.PDFire.Watermark;
using PDFiumSharp;
using PDFiumSharp.Enums;
using PDFiumSharp.Types;
using PdfDocument = iText.Kernel.Pdf.PdfDocument;

namespace NooberCong.PDFire.PDF;

public class PDFireDocument : IDisposable
{
    private MemoryStream _fileStream;

    public PDFireDocument(Stream fileStream)
    {
        var memStream = new MemoryStream();
        fileStream.CopyTo(memStream);

        if (!memStream.ToArray().IsPdf())
        {
            throw new InvalidPDFException();
        }

        _fileStream = memStream;
    }

    public PDFireDocument(string filePath)
    {
        var bytes = File.ReadAllBytes(filePath);

        if (!bytes.IsPdf())
        {
            throw new InvalidPDFException();
        }

        _fileStream = new MemoryStream(bytes);
    }

    public PDFireDocument(Uri fileUri)
    {
        var bytes = fileUri.DownloadData();

        if (!bytes.IsPdf())
        {
            throw new InvalidPDFException();
        }

        _fileStream = new MemoryStream(bytes);
    }

    public IEnumerable<PDFireBookmark> GetBookmarks()
    {
        using var doc = new PdfDocument(new PdfReader(_fileStream));

        PdfNameTree destsTree = doc.GetCatalog().GetNameTree(PdfName.Dests);

        var outlines = doc.GetOutlines(false)?.GetAllChildren()
                           ?.Select(ch => ParseBookmarksRecursive(ch, destsTree.GetNames(), doc))
                       ?? Array.Empty<PDFireBookmark>();

        _fileStream.Position = 0;

        return outlines;
    }

    public int GetPageCount()
    {
        using var doc = new PdfDocument(new PdfReader(_fileStream));
        var pageCount = doc.GetNumberOfPages();
        _fileStream.Position = 0;

        return pageCount;
    }

    public void RenderToImages(string pathPattern, IEnumerable<int> pageNumbers, ImageRenderingOptions options = null)
    {
        using var pageNumberIterator = pageNumbers.GetEnumerator();
        foreach (var imageBytes in RenderToImageBytes(pageNumbers, options))
        {
            pageNumberIterator.MoveNext();
            File.WriteAllBytes(string.Format(pathPattern, pageNumberIterator.Current), imageBytes);
        }
    }

    public byte[][] RenderToImageBytes(IEnumerable<int> pageNumbers, ImageRenderingOptions options = null)
    {
        options ??= new();

        var fileRead = FPDF_FILEREAD.FromStream(_fileStream);
        using var doc = new PDFiumSharp.PdfDocument(_fileStream, fileRead);

        var imageBytes = pageNumbers.Select(pn => Render(doc.Pages[pn - 1], options)).ToArray();

        _fileStream.Position = 0;
        return imageBytes;
    }

    public Image[] RenderToImages(IEnumerable<int> pageNumbers, ImageRenderingOptions options = null)
    {
        return RenderToImageBytes(pageNumbers, options).Select(bytes => Image.FromStream(new MemoryStream(bytes)))
            .ToArray();
    }

    public void RenderToImages(string pathPattern, int startPageNumber, int endPageNumber,
        ImageRenderingOptions options = null)
    {
        if (startPageNumber < endPageNumber)
        {
            throw new ArgumentOutOfRangeException(nameof(startPageNumber),
                "Must be lower or equal to endPageNumber");
        }

        RenderToImages(pathPattern, Enumerable.Range(startPageNumber, endPageNumber - startPageNumber + 1),
            options);
    }

    public byte[][] RenderToImageBytes(int startPageNumber, int endPageNumber, ImageRenderingOptions options = null)
    {
        if (startPageNumber < endPageNumber)
        {
            throw new ArgumentOutOfRangeException(nameof(startPageNumber),
                "Must be lower or equal to endPageNumber");
        }

        return RenderToImageBytes(Enumerable.Range(startPageNumber, endPageNumber - startPageNumber + 1), options);
    }

    public Image[] RenderToImages(int startPageNumber, int endPageNumber, ImageRenderingOptions options = null)
    {
        if (startPageNumber < endPageNumber)
        {
            throw new ArgumentOutOfRangeException(nameof(startPageNumber),
                "Must be lower or equal to endPageNumber");
        }

        return RenderToImages(Enumerable.Range(startPageNumber, endPageNumber - startPageNumber + 1), options);
    }

    public void AddImageWatermark(int startPageNumber, int endPageNumber, PDFireWatermarkImage watermarkImage,
        WatermarkOptions options = null)
    {
        if (startPageNumber < endPageNumber)
        {
            throw new ArgumentOutOfRangeException(nameof(startPageNumber),
                "Must be lower or equal to endPageNumber");
        }

        AddImageWatermark(Enumerable.Range(startPageNumber, endPageNumber - startPageNumber + 1), watermarkImage,
            options);
    }

    public void AddImageWatermark(IEnumerable<int> pageNumbers, PDFireWatermarkImage watermarkImage,
        WatermarkOptions options = null)
    {
        options ??= new();

        var rotatedImageBytes = watermarkImage.GetImage().Rotate(options.RotationDegree);

        using var destStream = new MemoryStream();

        using var doc = new PdfDocument(
            new PdfReader(_fileStream),
            new PdfWriter(destStream,
                new WriterProperties().SetCompressionLevel(CompressionConstants.BEST_COMPRESSION)
                    .SetFullCompressionMode(true)));

        var gs1 = new PdfExtGState();

        // Blur image
        gs1.SetFillOpacity(options.Opacity);

        var img = ImageDataFactory.Create(rotatedImageBytes);
        float imgWidth = img.GetWidth();
        float imgHeight = img.GetHeight();

        foreach (var pageNumber in pageNumbers)
        {
            var page = doc.GetPage(pageNumber);
            var pagesize = page.GetPageSize();

            var pageWidth = pagesize.GetWidth();
            var pageHeight = pagesize.GetHeight();

            // Scale image
            var scaleRatio = (pageWidth / imgWidth) * options.WidthRelativeToPageWidth;
            var scaledWidth = imgWidth * scaleRatio;
            var scaledHeight = imgHeight * scaleRatio;

            // Coordinates for middle of page
            var (x, y) = GetWatermarkCoordinates(options.Position, pageWidth, pageHeight, scaledWidth,
                scaledHeight);

            var canvas = new PdfCanvas(doc.GetPage(pageNumber));

            canvas.SaveState();
            canvas.SetExtGState(gs1);

            canvas.AddImageWithTransformationMatrix(img, scaledWidth, 0, 0, scaledHeight, x, y);
            canvas.RestoreState();
        }

        doc.Close();
        _fileStream = new MemoryStream(destStream.ToArray());
    }

    public void Save(string filePath)
    {
        File.WriteAllBytes(filePath, _fileStream.ToArray());
    }

    public void Save(Stream stream)
    {
        _fileStream.CopyTo(stream);
        _fileStream.Position = 0;
    }

    public byte[] ToBytes()
    {
        return _fileStream.ToArray();
    }

    public void Dispose()
    {
        _fileStream.Dispose();
    }

    private PDFireBookmark ParseBookmarksRecursive(PdfOutline outline, IDictionary<string, PdfObject> names,
        PdfDocument doc)
    {
        var bookmark = new PDFireBookmark
        {
            Title = outline.GetTitle()
        };

        if (outline.GetDestination() != null)
        {
            bookmark.PageNumber =
                doc.GetPageNumber((PdfDictionary) outline.GetDestination().GetDestinationPage(names));
        }

        bookmark.Children = outline.GetAllChildren().Select(ch => ParseBookmarksRecursive(ch, names, doc)).ToList();

        return bookmark;
    }

    private (float x, float y) GetWatermarkCoordinates(WatermarkPosition position, float pageWidth,
        float pageHeight,
        float imageWidth, float imageHeight)
    {
        switch (position)
        {
            case WatermarkPosition.TopLeft:
                return (0, pageHeight - imageHeight);
            case WatermarkPosition.TopRight:
                return (pageWidth - imageWidth, pageHeight - imageHeight);
            case WatermarkPosition.Center:
                return ((pageWidth - imageWidth) / 2, (pageHeight - imageHeight) / 2);
            case WatermarkPosition.BottomLeft:
                return (0, 0);
            case WatermarkPosition.BottomRight:
                return (pageWidth - imageWidth, 0);
            default:
                throw new ArgumentOutOfRangeException(nameof(position), position, null);
        }
    }

    private byte[] Render(PDFiumSharp.PdfPage page, ImageRenderingOptions options)
    {
        // Rescale Ratio
        var ratio = options.Width / page.Width;
        using var bm = new PDFiumBitmap((int) (page.Width * ratio), (int) (page.Height * ratio), true);

        // Fill background with white color
        bm.Fill(new FPDF_COLOR(255, 255, 255));

        // Render page to bitmap
        page.Render(bm, PageOrientations.Normal, RenderingFlags.LcdText);

        var image = Image.FromStream(bm.AsBmpStream());

        return image.Compress(compressionRate: GetCompressionRateForImageQuality(options.Quality));
    }

    private int GetCompressionRateForImageQuality(ImageQuality quality)
    {
        return quality switch
        {
            ImageQuality.High => 100,
            ImageQuality.Medium => 75,
            ImageQuality.Low => 50,
            _ => throw new ArgumentOutOfRangeException(nameof(quality), quality, null)
        };
    }
}