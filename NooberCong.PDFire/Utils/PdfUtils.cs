using System.Text;

namespace NooberCong.PDFire.Utils
{
    public static class PdfUtils
    {
        public static bool IsPdf(this byte[] fileBytes)
        {
            var pdfHeaderBytes = Encoding.ASCII.GetBytes("%PDF-");

            return pdfHeaderBytes.SequenceEqual(fileBytes.Take(pdfHeaderBytes.Length));
        }
    }
}