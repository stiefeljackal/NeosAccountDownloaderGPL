using MimeDetective.Storage;
using System.Collections.Immutable;

namespace AccountDownloaderLibrary.Mime;

/// <summary>
/// A class that contains Neos-spicific file definition types.
/// </summary>
public class CustomTypes
{
    // Based on: https://github.com/MediatedCommunications/Mime-Detective/blob/main/src/MimeDetective/Definitions/Default/FileTypes/Default.FileTypes.Audio.cs

    /// <summary>
    /// Creates a MeshX definition that can be used to analyze MashX files.
    /// </summary>
    /// <returns>The MeshX file type definition.</returns>
    public static ImmutableArray<Definition> MESHX() =>
        new List<Definition>() {
            new() {
                File = new() {
                    Extensions = new[]{"meshx"}.ToImmutableArray(),
                    MimeType = "application/meshx"
                },
                Signature = new Segment[] {
                    PrefixSegment.Create(0, "05 4D 65 73 68 58"),
                }.ToSignature(),
            },
        }.ToImmutableArray();

    /// <summary>
    /// Create an AnimX definition that can be used to analyze AnimX files.
    /// </summary>
    /// <returns>The AnimX file type definition.</returns>
    public static ImmutableArray<Definition> ANIMX() =>
        new List<Definition>() {
            new() {
                File = new() {
                    Extensions = new[]{"animx"}.ToImmutableArray(),
                    MimeType = "application/octet-stream"
                },
                Signature = new Segment[] {
                    PrefixSegment.Create(0, "05 41 6E 69 6D 58"),
                }.ToSignature()
            },
        }.ToImmutableArray();
}
