using AccountDownloaderLibrary.Implementations;
using CloudX.Shared;
using Moq;
using System.IO;
using System.IO.Abstractions.TestingHelpers;
using System.Net;
using System.Text.Json;

namespace AccountDownloaderLibrary.Test;

public class LocalAccountDataStore_StoreAsset
{
    [Theory]
    [MemberData(nameof(RandomArrayOfByteArrays))]
    public async void StoreAsset_AssetWithMimeType_StoresAssetInFileByMimeType(byte[] mockDataBytes)
    {
        Random.Shared.NextBytes(mockDataBytes);
        var mockHash = "20a2ae9876f205fa324fd51ed56a58aeee0e71eaee6bfbb2a556";
        var mockMimeType = "application/mock";
        var mockUri = new Uri("https://api.neos.com/fake/endpoint");
        var mockAssetsPath = "mock/assets/path";
        var mockAssetMetadata = new AssetMetadata(mockUri, mockMimeType, mockDataBytes.LongLength);
        using var mockStream = new MemoryStream(mockDataBytes);

        var tuple = Utility.CreateLocalAccountDataStoreTuple(assetsPath: mockAssetsPath);
        tuple.fsMock.AddDirectory(mockAssetsPath);

        tuple.mimeDetectorMock.Setup(d => d.GetFileExtensionByMimeType(It.Is<string>(mime => mime == mockMimeType))).Returns("mock");

        await tuple.store.StoreAsset(mockAssetMetadata, mockStream, mockHash);

        tuple.mimeDetectorMock.Verify(d => d.MostLikelyFileExtension(It.IsAny<byte[]>()), Times.Never);

        var writtenAssetBytes = tuple.fsMock.GetFile($"{mockAssetsPath}/{mockHash}.mock").Contents;

        Assert.Equal(mockDataBytes, writtenAssetBytes);
    }

    [Theory]
    [MemberData(nameof(RandomArrayOfByteArrays))]
    public async void StoreAsset_AssetWithoutMimeType_StoresAssetInFileByHeaderMagic(byte[] mockDataBytes)
    {
        Random.Shared.NextBytes(mockDataBytes);
        var mockHash = "20a2ae9876f205fa324fd51ed56a58aeee0e71eaee6bfbb2a556";
        var mockMimeType = "application/mock";
        var mockUri = new Uri("https://api.neos.com/fake/endpoint");
        var mockAssetsPath = "mock/assets/path";
        var mockAssetMetadata = new AssetMetadata(mockUri, mockMimeType, mockDataBytes.LongLength);
        using var mockStream = new MemoryStream(mockDataBytes);

        var tuple = Utility.CreateLocalAccountDataStoreTuple(assetsPath: mockAssetsPath);
        tuple.fsMock.AddDirectory(mockAssetsPath);

        tuple.mimeDetectorMock.Setup(d => d.GetFileExtensionByMimeType(It.Is<string>(mime => mime == mockMimeType))).Returns("");
        tuple.mimeDetectorMock.Setup(d => d.MostLikelyFileExtension(It.IsAny<byte[]>())).Returns("mock");

        await tuple.store.StoreAsset(mockAssetMetadata, mockStream, mockHash);

        var writtenAssetBytes = tuple.fsMock.GetFile($"{mockAssetsPath}/{mockHash}.mock").Contents;

        Assert.Equal(mockDataBytes, writtenAssetBytes);
    }

    [Fact]
    public async void StoreAsset_NonExistentDirectory_ThrowsExceptionOnDirectoryNotFound()
    {
        var mockHash = "20a2ae9876f205fa324fd51ebe6bfbb2a556";
        var mockAssetsPath = "mock/assets/path";
        var mockAssetData = new AssetMetadata();
        using var mockStream = new MemoryStream();

        var tuple = Utility.CreateLocalAccountDataStoreTuple(assetsPath: mockAssetsPath);
        await Assert.ThrowsAsync<DirectoryNotFoundException>(async () => await tuple.store.StoreAsset(mockAssetData, mockStream,mockHash));
    }

    public static IEnumerable<object[]> RandomArrayOfByteArrays =>
        new List<object[]>
        {
            new object[] { new byte[25] },
            new object[] { new byte[50] },
            new object[] { new byte[100] },
            new object[] { new byte[200] },
            new object[] { new byte[400] }
        };
}
