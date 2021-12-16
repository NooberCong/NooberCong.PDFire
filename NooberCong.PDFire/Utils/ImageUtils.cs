using System.Drawing;
using System.Drawing.Imaging;
using System.Text;
using Encoder = System.Drawing.Imaging.Encoder;

namespace NooberCong.PDFire.Utils
{
    public static class ImageUtils
    {
        public static bool IsImage(this byte[] bytes)
        {
            var imageHeaderBytes = new byte[][]
            {
                Encoding.ASCII.GetBytes("BM"), // BMP
                new byte[] {137, 80, 78, 71}, // PNG
                new byte[] {73, 73, 42}, // TIFF
                new byte[] {77, 77, 42}, // TIFF2
                new byte[] {255, 216, 255, 224}, // jpeg
                new byte[] {255, 216, 255, 225}, // jpeg canon
            };

            foreach (var headerBytes in imageHeaderBytes)
            {
                if (headerBytes.SequenceEqual(bytes.Take(headerBytes.Length)))
                {
                    return true;
                }
            }

            return false;
        }

        public static byte[] ToBytes(this Image image)
        {
            using var memStream = new MemoryStream();
            image.Save(memStream, ImageFormat.Png);
            return memStream.ToArray();
        }

        public static byte[] Compress(this Image image, int compressionRate)
        {
            if (compressionRate <= 0 || compressionRate > 100)
            {
                throw new Exception("Compression rate must be between 1 and 100");
            }

            var encoder = ImageCodecInfo.GetImageEncoders().First(ec => ec.FormatID == ImageFormat.Png.Guid);
            Encoder myEncoder = Encoder.Quality;
            EncoderParameters encoderParams = new EncoderParameters(1);
            encoderParams.Param[0] = new EncoderParameter(myEncoder, compressionRate);

            using var memStream = new MemoryStream();
            image.Save(memStream, encoder, encoderParams);

            return memStream.ToArray();
        }

        public static byte[] Rotate(this Image image, float degree)
        {
            var (rotatedWith, rotatedHeight) = GetRotatedSize(degree, image.Width, image.Height);
            var rotatedImage = new Bitmap(rotatedWith, rotatedHeight);
            rotatedImage.SetResolution(image.HorizontalResolution, image.VerticalResolution);

            using (Graphics g = Graphics.FromImage(rotatedImage))
            {
                // Set the rotation point to the center in the matrix
                g.TranslateTransform(rotatedWith / 2f, rotatedHeight / 2f);
                // Rotate
                g.RotateTransform(-degree);
                // Restore rotation point in the matrix
                g.TranslateTransform(-rotatedWith / 2f, -rotatedHeight / 2f);
                // Draw the image on the bitmap
                g.DrawImage(image,
                    new System.Drawing.Point((rotatedWith - image.Width) / 2, (rotatedHeight - image.Height) / 2));

                g.Flush();
            }

            using var memStream = new MemoryStream();
            rotatedImage.Save(memStream, ImageFormat.Png);

            return memStream.ToArray();
        }

        private static (int, int) GetRotatedSize(double deg, int orgWidth, int orgHeight)
        {
            double rads = Math.PI / 180 * deg;
            var xs = new double[] {0, orgWidth, orgWidth, 0};
            var ys = new double[] {0, 0, orgHeight, orgHeight};

            var cos = Math.Cos(rads);
            var sin = Math.Sin(rads);

            for (int i = 0; i < 4; i++)
            {
                var x = xs[i];
                var y = ys[i];
                xs[i] = cos * x - sin * y;
                ys[i] = sin * x + cos * y;
            }

            return ((int) Math.Ceiling(xs.Max() - xs.Min()), (int) Math.Ceiling(ys.Max() - ys.Min()));
        }
    }
}