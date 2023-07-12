using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;

namespace AccountDownloaderLibrary;

/// <summary>
/// A data model of the metadata for an assset.
///
/// TODO: Create custom JsonConverter before making this a readonly struct.
/// </summary>
public struct AssetMetadata
{
    /// <summary>
    /// The url location of the asset based on its last gather.
    /// </summary>
    [JsonPropertyName("url")]
    public Uri Url { get; set; }

    /// <summary>
    /// The mime type of the asset.
    /// </summary>
    [JsonPropertyName("mimeType")]
    public string MimeType { get; set; }

    /// <summary>
    /// The size of the asset in bytes.
    /// </summary>
    [JsonPropertyName("size")]
    public long Size { get; set; }

    public AssetMetadata(string mimeType) : this(null, mimeType) { }

    public AssetMetadata(Uri url, string mimeType) : this(url, mimeType, 0L) { }

    public AssetMetadata(Uri url, string mimeType, long size)
    {
        Url = url;
        MimeType = mimeType;
        Size = size;
    }

    public static bool operator ==(AssetMetadata left, AssetMetadata right) =>
        left.Url == right.Url && left.Size == right.Size && left.MimeType == right.MimeType;

    public static bool operator !=(AssetMetadata left, AssetMetadata right) => !(left == right);

    public override bool Equals([NotNullWhen(true)] object obj)
    {
        return obj is AssetMetadata && this == (AssetMetadata)obj;
    }

    public override int GetHashCode()
    {
        return Url.GetHashCode() ^ Size.GetHashCode() * MimeType.GetHashCode();
    }
}

