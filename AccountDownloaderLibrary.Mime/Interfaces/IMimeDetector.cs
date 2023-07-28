using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AccountDownloaderLibrary.Mime.Interfaces
{
    public interface IMimeDetector
    {
        string? MostLikelyFileExtension(string filePath);

        string? MostLikelyFileExtension(Stream stream);

        string? MostLikelyFileExtension(byte[] bytes);

        string? MostLikelyMimeType(string filePath);

        string? MostLikelyMimeType(Stream stream);

        string? MostLikelyMimeType(byte[] bytes);

        string? GetFileExtensionByMimeType(string mimeType);

        string? GetMimeTypeByFileExtension(string extensionType);
    }
}
