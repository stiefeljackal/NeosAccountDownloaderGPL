using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace AccountDownloaderLibrary.Extensions;

public static class FilenameExtensions
{
    private static readonly Regex FILE_EXTENSION_REGEX = new Regex("^.+\\.(?<ext>.+)$");
    public static string GetFileExtensionFromName(this string filename)
    {
        var match = FILE_EXTENSION_REGEX.Match(filename);

        return match.Length > 0 ? match.Groups["ext"].Value : string.Empty;
    }
}
