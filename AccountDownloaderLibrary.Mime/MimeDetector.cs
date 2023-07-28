using MimeDetective;
using MimeDetective.Definitions;
using MimeDetective.Engine;
using MimeDetective.Storage;
using System.Collections.Immutable;

namespace AccountDownloaderLibrary.Mime;

using Interfaces;
using Internal;
using System.IO.Abstractions;

public class MimeDetector : IMimeDetector
{
    public const short MAX_FILE_SIZE_TO_READ = 1028;

    public static readonly MimeDetector Instance = new MimeDetector();

    public static readonly ContentReader DefaultReader = new ContentReader()
    {
        MaxFileSize = MAX_FILE_SIZE_TO_READ
    };

    private readonly IFileSystem _fileSystem;

    private readonly ContentInspector _inspector;

    private readonly MimeTypeToFileExtensionLookup _mimeToExtensionLookup;

    private readonly FileExtensionToMimeTypeLookup _extensionToMimeLookup;

    public MimeDetector(IFileSystem fileSystem)
    {
        _fileSystem = fileSystem;

        ImmutableArray<Definition> exhaustiveDefs = new ExhaustiveBuilder()
        {
            UsageType = MimeDetective.Definitions.Licensing.UsageType.PersonalNonCommercial
        }.Build();

        ImmutableArray<Definition>.Builder AllBuildier = ImmutableArray.CreateBuilder<Definition>();

        AllBuildier.AddRange(CustomTypes.SEVENZBSON());
        AllBuildier.AddRange(Default.All());
        AllBuildier.AddRange(exhaustiveDefs);
        AllBuildier.AddRange(CustomTypes.MESHX());
        AllBuildier.AddRange(CustomTypes.ANIMX());

        var all = AllBuildier
            .TrimDescription()
            .TrimMeta()
            .ToImmutableArray();

        _inspector = new ContentInspectorBuilder()
        {
            Definitions = all
        }.Build();

        _mimeToExtensionLookup = new MimeTypeToFileExtensionLookupBuilder()
        {
            Definitions = all
        }.Build();

        _extensionToMimeLookup = new FileExtensionToMimeTypeLookupBuilder()
        {
            Definitions = all
        }.Build();
    }

    public MimeDetector() : this(new FileSystem()) { }

    public string? MostLikelyFileExtension(string filePath)
    {
        using var fileStream = _fileSystem.File.OpenRead(filePath);
        var ext = MostLikelyFileExtension(fileStream);

        return ext;
    }

    public string? MostLikelyFileExtension(Stream stream) =>
        _inspector.Inspect(stream, stream.CanSeek, DefaultReader).ByFileExtension().ChooseMostLikely();

    public string? MostLikelyFileExtension(byte[] bytes) =>
        _inspector.Inspect(bytes).ByFileExtension().ChooseMostLikely();


    public string? MostLikelyMimeType(string filePath)
    {
        using var fileStream = _fileSystem.File.OpenRead(filePath);
        var ext = MostLikelyMimeType(fileStream);

        return ext;
    }

    public string? MostLikelyMimeType(Stream stream) =>
        _inspector.Inspect(stream, stream.CanSeek, DefaultReader).ByMimeType().ChooseMostLikely();

    public string? MostLikelyMimeType(byte[] bytes) =>
        _inspector.Inspect(bytes).ByMimeType().ChooseMostLikely();


    public string? GetFileExtensionByMimeType(string? mimeType) {
        switch (mimeType)
        {
            case null:
            case "":
            case "application/octet-stream":
                return null;
            default:
                var extensions = _mimeToExtensionLookup.TryGetValues(mimeType);
                return extensions.Length > 0 ? extensions.ChooseMostLikely() : null;
        }
    }

    public string? GetMimeTypeByFileExtension(string fileExtension)
    {
        switch (fileExtension)
        {
            case null:
            case "":
                return null;
            default:
                var mimeTypes = _extensionToMimeLookup.TryGetValues(fileExtension);
                return mimeTypes.Length > 0 ? mimeTypes.ChooseMostLikely() : null;
        }
    }
}
