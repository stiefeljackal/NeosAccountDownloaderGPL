using MimeDetective.Engine;
using System.Collections.Immutable;

namespace AccountDownloaderLibrary.Mime.Internal;

internal static class MimeDetectorExtensions
{
    internal static string? ChooseMostLikely(this ImmutableArray<FileExtensionMatch> results) => results.FirstOrDefault()?.Extension;

    internal static string? ChooseMostLikely(this ImmutableArray<MimeTypeMatch> results) => results.FirstOrDefault()?.MimeType;
}
