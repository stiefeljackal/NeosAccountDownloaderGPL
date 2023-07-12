using AccountDownloaderLibrary.Implementations;
using CloudX.Shared;
using Moq;
using System.IO;
using System.IO.Abstractions.TestingHelpers;
using System.Net;

namespace AccountDownloaderLibrary.Test;

public class LocalAccountDataStore_GetAssetMetadata
{
    [Theory]
    [InlineData("application/mock")]
    [InlineData("")]
    public async void GetAssetMetadata_HashValue_ReturnsAssetMetadata(string mockMimeType)
    {
        var mockHash = "20a2ae9876f205fa324fd51ed56a58aeee0e71eaee6bfbb2a556";
        var mockAssetsPath = "mock/assets/path";
        var mockSize = Random.Shared.Next();

        var tuple = Utility.CreateLocalAccountDataStoreTuple(assetsPath: mockAssetsPath);
        var fsMock = tuple.fsMock;

        var mockFileData = Utility.CreateJsonFile(new AssetMetadata(null, mockMimeType, mockSize));
        fsMock.AddFile($"{mockAssetsPath}/{mockHash}.metadata.json", mockFileData);

        var assetMetadata = await tuple.store.GetAssetMetadata(mockHash);

        Assert.Equal(mockSize, assetMetadata.Size);
        Assert.Equal(mockMimeType, assetMetadata.MimeType);
    }

    [Fact]
    public async void GetAssetSize_NonExistentHashForMetadataFile_ThrowsFileNotFoundException()
    {
        var mockHash = "20aafa324fd51ed56a58aeee0e71eaee6bfbb2a556";
        var mockAssetsPath = "mock/assets/path";

        var tuple = Utility.CreateLocalAccountDataStoreTuple(assetsPath: mockAssetsPath);

        await Assert.ThrowsAsync<FileNotFoundException>(async () => await tuple.store.GetAssetMetadata(mockHash));
    }

    [Fact]
    public async void GetAssetMetadata_HashWithNoMimeValue_ReturnsAssetMetadataFromActualFile()
    {
        var mockHash = "20a2ae9876f205fa324fd5ae0e71eaee6bfbb2a556";
        var mockAssetsPath = "mock/assets/path";
        var mockData = "Mock\nData";
        var mockMimeType = "application/mime";

        var tuple = Utility.CreateLocalAccountDataStoreTuple(assetsPath: mockAssetsPath);
        var fsMock = tuple.fsMock;

        var mockMetadataFileData = Utility.CreateJsonFile<AssetMetadata>(new AssetMetadata(null, null));
        fsMock.AddFile($"{mockAssetsPath}/{mockHash}.metadata.json", mockMetadataFileData);
        var mockAssetFileData = new MockFileData(mockData);
        fsMock.AddFile($"{mockAssetsPath}/{mockHash}.mock", mockAssetFileData);

        tuple.mimeDetectorMock.Setup(d => d.MostLikelyMimeType(It.IsAny<Stream>())).Returns(mockMimeType);

        var assetMetadata = await tuple.store.GetAssetMetadata(mockHash);

        Assert.Equal(mockAssetFileData.Contents.LongLength, assetMetadata.Size);
        Assert.Equal(mockMimeType, assetMetadata.MimeType);
    }

    [Fact]
    public async void GetAssetSize_NonExistentHashForDataFile_ThrowsFileNotFoundException()
    {
        var mockHash = "20a2ae9876f20324fd5ae0e71eaee6bfbb2a556";
        var mockAssetsPath = "mock/assets/path";

        var tuple = Utility.CreateLocalAccountDataStoreTuple(assetsPath: mockAssetsPath);
        var fsMock = tuple.fsMock;

        var mockMetadataFileData = Utility.CreateJsonFile<AssetMetadata>(new AssetMetadata(null, null));
        fsMock.AddFile($"{mockAssetsPath}/{mockHash}.metadata.json", mockMetadataFileData);

        await Assert.ThrowsAsync<FileNotFoundException>(async () => await tuple.store.GetAssetMetadata(mockHash));
    }
}
