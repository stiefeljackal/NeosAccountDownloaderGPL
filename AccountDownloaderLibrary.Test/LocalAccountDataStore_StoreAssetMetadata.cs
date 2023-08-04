using AccountDownloaderLibrary.Implementations;
using CloudX.Shared;
using Moq;
using System.IO;
using System.IO.Abstractions.TestingHelpers;
using System.Net;
using System.Text.Json;

namespace AccountDownloaderLibrary.Test;

public class LocalAccountDataStore_StoreAssetMetadata
{
    [Fact]
    public async void StoreAssetMetadata_AssetMetadata_StoresAssetMetadataInFile()
    {
        var mockHash = "20a2ae9876f205fa324fd51ebe6bfbb2a556";
        var mockAssetsPath = "mock/assets/path";
        var mockMimeType = "application/mock";
        var mockSize = Random.Shared.Next();
        var mockUri = new Uri("https://api.neos.com/fake/endpoint");
        var mockAssetData = new AssetMetadata(mockUri, mockMimeType, mockSize);

        var tuple = Utility.CreateLocalAccountDataStoreTuple(assetsPath: mockAssetsPath);
        tuple.fsMock.AddDirectory($"{mockAssetsPath}Metadata");

        await tuple.store.StoreAssetMetadata(mockAssetData, mockHash);

        var writtenMetadata = JsonSerializer.Deserialize<AssetMetadata>(tuple.fsMock.GetFile($"{mockAssetsPath}Metadata/{mockHash}.metadata.json").Contents);

        Assert.Equal(writtenMetadata, mockAssetData);
    }

    [Fact]
    public async void StoreAssetMetadata_NonExistentDirectory_ThrowsExceptionOnDirectoryNotFound()
    {
        var mockHash = "20a2ae9876f205fa324fd51ebe6bfbb2a556";
        var mockAssetsPath = "mock/assets/path";
        var mockAssetData = new AssetMetadata();

        var tuple = Utility.CreateLocalAccountDataStoreTuple(assetsPath: mockAssetsPath);
        await Assert.ThrowsAsync<DirectoryNotFoundException>(async () => await tuple.store.StoreAssetMetadata(mockAssetData, mockHash));
    }
}
