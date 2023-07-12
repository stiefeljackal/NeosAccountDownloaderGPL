using AccountDownloaderLibrary.Implementations;
using CloudX.Shared;
using Moq;
using System.IO;
using System.IO.Abstractions.TestingHelpers;

namespace AccountDownloaderLibrary.Test;

public class LocalAccountDataStore_GetAssetStream
{ 
    [Fact]
    public async void GetAssetStream_HashValue_ReturnsStreamForFile()
    {
        var mockHash = "20a2ae9876f205fa324fd51ed56a58aeee0e71eaee6bfbb2a556";
        var mockAssetsPath = "mock/assets/path";
        var mockData = "Mock\nData";

        var tuple = Utility.CreateLocalAccountDataStoreTuple(assetsPath: mockAssetsPath);
        var fsMock = tuple.fsMock;

        var mockFileData = new MockFileData(mockData);
        fsMock.AddFile($"{mockAssetsPath}/{mockHash}.mock", mockFileData);


        using (var stream = await tuple.store.GetAssetStream(mockHash))
        using (var reader = new BinaryReader(stream))
        {
            Assert.Equal(0L, stream.Position);
            var actualBytes = reader.ReadBytes((int)stream.Length);
            Assert.Equal(mockFileData.Contents, actualBytes);
        }
    }

    [Fact]
    public async void GetAssetSize_NonExistentHash_ThrowsFileNotFoundException()
    {
        var mockHash = "20a276f205fa324fd51ed56a58aeee0e71eaee6bfbb2a556";
        var mockAssetsPath = "mock/assets/path";

        var tuple = Utility.CreateLocalAccountDataStoreTuple(assetsPath: mockAssetsPath);

        await Assert.ThrowsAsync<FileNotFoundException>(async () => await tuple.store.GetAssetMetadata(mockHash));
    }

    [Fact]
    public async void GetAssetStream_NonExistentHash_ThrowsExceptionOnFileNotFound()
    {
        var mockHash = "20a2ae9876f205fa324fd51ed56a58aeee0e71eaee6bfbb2a556";
        var mockAssetsPath = "mock/assets/path";

        var tuple = Utility.CreateLocalAccountDataStoreTuple(assetsPath: mockAssetsPath);
        tuple.fsMock.AddDirectory(mockAssetsPath);

        await Assert.ThrowsAsync<FileNotFoundException>(async () =>
        {
            await tuple.store.GetAssetStream(mockHash);
        });
    }

    [Theory]
    [InlineData("png", "txt", "txt")]
    [InlineData("txt", "docx", "animx", "jpg")]
    [InlineData("meshx", "webp")]
    public async void GetAssetStream_MultipleHashExtensions_ThrowsExceptionOnMultipleExtensions(params string[] extensions)
    {
        var mockHash = "20a2ae9876f205fa324fd51ed56a58aeee0e71eaee6bfbb2a556";
        var mockAssetsPath = "mock/assets/path";

        var tuple = Utility.CreateLocalAccountDataStoreTuple(assetsPath: mockAssetsPath);
        var fsMock = tuple.fsMock;

        foreach (var extension in extensions)
        {
            fsMock.AddFile($"{mockAssetsPath}/{mockHash}.{extension}", new MockFileData("mock"));
        }

        await Assert.ThrowsAsync<MultipleHashExtensionsException>(async () =>
        {
            await tuple.store.GetAssetStream(mockHash);
        });
    }
}
