using System.Net;

namespace NooberCong.PDFire.Utils;

public static class NetworkUtils
{
    public static byte[] DownloadData(this Uri fileUri)
    {
        WebClient client = new WebClient();
        return client.DownloadData(fileUri);
    }
}