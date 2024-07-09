using System.IO;
using System.Reflection;
using System.Text.RegularExpressions;
using SharpCompress;
using SharpCompress.Archives;
using SharpCompress.Archives.SevenZip;

namespace AccountDownloaderLibrary.NeosFetch;

public static class NeosFetcher
{
    private static readonly HashSet<string> NEOS_ASSEMBLY_NAMES_SET = new HashSet<string> { "BaseX", "Ben.Demystifier", "CloudX.Shared", "CodeX", "Octokit" };

    private const string SEVENZIP_DOWNLOAD_LOCATION = "https://assets.neos.com/install/Pro/Data/2022.1.28.1310_YTDLP.7z";

    private static readonly Regex DATA_MANAGED_ASSEMBLY_REGEX = new Regex("Neos_Data/Managed/(.+)\\.dll");

    /// <summary>
    /// Checks if the current directory that the executable is located contains the necessary
    /// Neos assembly files.
    /// </summary>
    /// <returns></returns>
    public static bool HasNeosAssembilesInExecutingPath()
    {
        var executingPath = GetExecutingPath();

        foreach (var assemblyName in NEOS_ASSEMBLY_NAMES_SET)
        {
            if (!File.Exists($"{executingPath}\\{assemblyName}.dll")) { return false; }
        }

        return true;
    }

    /// <summary>
    /// Unzips and saves the Neos assembly files on the current directory that the executable
    /// is located at. This is required as bundling these files will break the EULA.
    /// </summary>
    /// <returns></returns>
    public static async Task EnsureNeosAssemblies()
    {
        var executingPath = GetExecutingPath();

        using (var client = new HttpClient())
        {
            var httpResponse = await client.GetAsync(SEVENZIP_DOWNLOAD_LOCATION).ConfigureAwait(false);
            var fileBytes = await httpResponse.Content.ReadAsByteArrayAsync().ConfigureAwait(false);

            var sevenZipFilePath = $"{executingPath}\\Neos.7z";

            using var archive = SevenZipArchive.Open(httpResponse.Content.ReadAsStream());

            foreach (var entry in archive.Entries.Where(entry => !entry.IsDirectory))
            {
                var key = entry.Key;
                if (key == null) { continue; }

                var matches = DATA_MANAGED_ASSEMBLY_REGEX.Matches(key);

                if (matches.Count <= 0) {  continue; }

                var filename = matches[0]?.Groups[1]?.Value;
                if (filename != null && NEOS_ASSEMBLY_NAMES_SET.Contains(filename))
                {
                    entry.WriteToDirectory(".");
                }
            }
        }
    }

    /// <summary>
    /// Get the executing path of the executable.
    /// </summary>
    /// <returns></returns>
    private static DirectoryInfo GetExecutingPath()
    {
        var location = Assembly.GetExecutingAssembly().Location;
        return string.IsNullOrEmpty(location) ? new DirectoryInfo("."): new FileInfo(location).Directory!;
    }
}
