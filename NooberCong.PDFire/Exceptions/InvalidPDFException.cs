namespace NooberCong.PDFire.Exceptions;

public class InvalidPDFException : Exception
{
    public InvalidPDFException() : base("File is currupted or is not a pdf")
    {
    }
}