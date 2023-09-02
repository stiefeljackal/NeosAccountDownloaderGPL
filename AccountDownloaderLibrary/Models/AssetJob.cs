using CloudX.Shared;

namespace AccountDownloaderLibrary.Models;

using Extensions;

readonly struct AssetJob
{
    public readonly NeosDBAsset asset;
    public readonly Record forRecord;
    public readonly IAccountDataGatherer source;
    public readonly RecordStatusCallbacks callbacks;

    public string RecordType { get => forRecord.RecordType; }

    public readonly string AssetExtension
    {
        get
        {
            var hash = asset.Hash;
            var assetUri = forRecord.AssetURI ?? string.Empty;
            var thumbnailUri = forRecord.ThumbnailURI ?? string.Empty;
            var isAssetUri = assetUri.Contains(hash);

            var neosDbUrl = isAssetUri ? assetUri : (thumbnailUri.Contains(hash) ? thumbnailUri : hash);

            var ext = neosDbUrl.GetFileExtensionFromName();

            if (string.IsNullOrEmpty(ext) && isAssetUri && (RecordType == "world" || RecordType == "object"))
            {
                ext = "7zbson";
            }

            return ext != string.Empty ? ext : null;
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
