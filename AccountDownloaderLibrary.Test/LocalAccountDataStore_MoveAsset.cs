using AccountDownloaderLibrary.Implementations;
using CloudX.Shared;
using Moq;
using System.IO;
using System.IO.Abstractions.TestingHelpers;
using System.Net;
using System.Text.Json;

namespace AccountDownloaderLibrary.Test;

public class LocalAccountDataStore_MoveAsset
{
    [Fact]
    public async void MoveAsset_OldPathAndNewPath_MoveFileToNewPath()
    {
        var mockHash = "20a2ae9876f205fa324fd51ed56a58aeee0e71eaee6bfbb2a556";
        var mockAssetsPath = "mock/assets/path";
        var mockData = "Mock\nData";
        var mockFileData = new MockFileData(mockData);
        var mockAssetFilePath = $"{mockAssetsPath}/{mockHash}";

        var expectedAssetFilePath = $"{mockAssetsPath}/{mockHash}.mock";

        var tuple = Utility.CreateLocalAccountDataStoreTuple(assetsPath: mockAssetsPath);
        tuple.fsMock.AddDirectory(mockAssetsPath);
        tuple.fsMock.AddFile(mockAssetFilePath, mockFileData);


        tuple.mimeDetectorMock.Setup(d => d.MostLikelyFileExtension(It.IsAny<Stream>())).Returns("mock");

        await tuple.store.MoveAsset(mockAssetFilePath, mockAssetFilePath);

        Assert.True(tuple.fsMock.FileExists(expectedAssetFilePath));
        Assert.False(tuple.fsMock.FileExists(mockAssetFilePath));

        var writtenAssetText = tuple.fsMock.GetFile(expectedAssetFilePath).TextContents;
        Assert.Equal(mockData, writtenAssetText);
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public async void MoveAsset_OldPathAndNewPathAreSame_DontMoveFile(string ext)
    {
        var mockHash = "20a2ae9876f205fa324fd51ed56a58aeee0e71eaee6bfbb2a556";
        var mockAssetsPath = "mock/assets/path";
        var mockData = "Mock\nData";
        var mockFileData = new MockFileData(mockData);
        var mockAssetFilePath = $"{mockAssetsPath}/{mockHash}";


        var tuple = Utility.CreateLocalAccountDataStoreTuple(assetsPath: mockAssetsPath);
        tuple.fsMock.AddFile(mockAssetFilePath, mockFileData);


        tuple.mimeDetectorMock.Setup(d => d.MostLikelyFileExtension(It.IsAny<Stream>())).Returns(ext);

        await tuple.store.MoveAsset(mockAssetFilePath, mockAssetFilePath);

        Assert.True(tuple.fsMock.FileExists(mockAssetFilePath));
    }

    [Fact]
    public async void MoveAsset_NonExistentFile_ThrowsExceptionOnFileNotFoundForMove()
    {
        var mockHash = "20a2ae9876f205fa324fd51ed56abaee6bfbb2a556";
        var mockAssetsPath = "mock/assets/path";
        var mockAssetFilePath = $"{mockAssetsPath}/{mockHash}";

        var tuple = Utility.CreateLocalAccountDataStoreTuple(assetsPath: mockAssetsPath);
        tuple.fsMock.AddDirectory(mockAssetsPath);

        await Assert.ThrowsAsync<FileNotFoundException>(async () => await tuple.store.MoveAsset(mockAssetFilePath, mockAssetFilePath));
    }

    [Fact]
    public async void MoveAsset_NonExistentDirectory_ThrowsExceptionOnDirectoryNotFoundToMoveTo()
    {
        var mockHash = "20a2ae9876f205fa324fd51ed56a58aeee0e71eaee6bfbb2a556";
        var mockAssetsPath = "mock/assets/path";
        var mockData = "Mock\nData";
        var mockFileData = new MockFileData(mockData);
        var mockAssetFilePath = $"{mockAssetsPath}/{mockHash}";

        var tuple = Utility.CreateLocalAccountDataStoreTuple(assetsPath: mockAssetsPath);
        tuple.fsMock.AddFile(mockAssetFilePath, mockFileData);


        tuple.mimeDetectorMock.Setup(d => d.MostLikelyFileExtension(It.IsAny<Stream>())).Returns("mock");

        await Assert.ThrowsAsync<DirectoryNotFoundException>(async () => await tuple.store.MoveAsset(mockAssetFilePath, "/fake/nonexistent/dir"));
    }
}
