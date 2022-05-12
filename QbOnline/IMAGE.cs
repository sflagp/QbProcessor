using System;
using System.Net.Mime;
using System.Text;

namespace QbModels.QBOProcessor
{
    public static class IMAGE
    {
        public static string FileExtension(this ContentType src) =>
            src.MediaType switch
            {
                MediaTypeNames.Image.Tiff => ".tif",
                MediaTypeNames.Image.Jpeg => ".jpg",
                MediaTypeNames.Image.Gif => ".gif",
                MediaTypeNames.Text.Xml => ".xml",
                MediaTypeNames.Text.Html => ".htm",
                MediaTypeNames.Text.Plain => ".txt",
                MediaTypeNames.Text.RichText => ".rtf",
                _ => ".pdf"
            };

        public static ContentType GetContentType(byte[] docData) => GetContentType(Encoding.ASCII.GetString(docData));

        public static ContentType GetContentType(string docData)
        {
            int maxSubString = Math.Min(docData.Length, 1024);

            if (docData.Substring(0, maxSubString).Contains("%PDF")) return new ContentType(MediaTypeNames.Application.Pdf);

            return new ContentType(MediaTypeNames.Image.Tiff);
        }
    }
}
