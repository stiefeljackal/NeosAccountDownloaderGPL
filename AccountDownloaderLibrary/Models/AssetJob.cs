using CloudX.Shared;

namespace AccountDownloaderLibrary.Models;

using Extensions;

readonly struct AssetJob
{
    public readonly NeosDBAsset asset;
    public readonly Record forRecord;
    public readonly IAccountDataGatherer source;
    public readonly RecordStatusCallbacks callbacks;

    public readonly string AssetExtension
    {
        get
        {
            var hash = asset.Hash;
            var assetUri = forRecord.AssetURI ?? string.Empty;
            var thumbnailUri = forRecord.ThumbnailURI ?? string.Empty;

            var neosDbUrl = assetUri.Contains(hash) ? assetUri : (thumbnailUri.Contains(hash) ? thumbnailUri : hash);

            return neosDbUrl.GetFileExtensionFromName();
        }
    }

    public AssetJob(Record forRecord, NeosDBAsset asset, IAccountDataGatherer source, RecordStatusCallbacks re)
    {
        this.asset = asset;
        this.source = source;
        this.callbacks = re;
        this.forRecord = forRecord;
    }
}
