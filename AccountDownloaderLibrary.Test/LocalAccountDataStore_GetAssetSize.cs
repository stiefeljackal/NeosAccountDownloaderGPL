using AccountDownloaderLibrary.Implementations;
using CloudX.Shared;
using Moq;
using System.IO;
using System.IO.Abstractions.TestingHelpers;
using System.Net;
using System.Text.Json;

namespace AccountDownloaderLibrary.Test;

public class LocalAccountDataStore_GetAssetSize
{ 
    [Fact]
    public async void GetAssetSize_HashValue_ReturnsTheAssetSize()
    {
        var mockHash = "20a2ae9876f205fa324fd51ed56aee0e71eaee6bfbb2a556";
        var mockAssetsPath = "mock/assets/path";
        var mockSize = Random.Shared.Next();

        var tuple = Utility.CreateLocalAccountDataStoreTuple(assetsPath: mockAssetsPath);
        var fsMock = tuple.fsMock;

        var mockFileData = Utility.CreateJsonFile(new AssetMetadata(null, string.Empty, mockSize));
        fsMock.AddFile($"{mockAssetsPath}Metadata/{mockHash}.metadata.json", mockFileData);

        var size = await tuple.store.GetAssetSize(mockHash);

        Assert.Equal(mockSize, size);
    }

    [Fact]
    public async void GetAssetSize_NonExistentHash_ThrowsFileNotFoundException()
    {
        var mockHash = "20a276f205fa324fd51ed56a58aeee0e71eaee6bfbb2a556";
        var mockAssetsPath = "mock/assets/path";

        var tuple = Utility.CreateLocalAccountDataStoreTuple(assetsPath: mockAssetsPath);

        await Assert.ThrowsAsync<FileNotFoundException>(async () => await tuple.store.GetAssetSize(mockHash));
    }
}
