using CloudX.Shared;
using MimeDetective.Storage;
using Moq;
using System;
using System.IO.Abstractions.TestingHelpers;
using System.Reflection;

namespace AccountDownloaderLibrary.Test;

// I don't really like all of this setup for mocking. If there is a better way to set this up, then I'm all for it.

public class LocalAccountDataStore_DownloadProcessor
{
    [Fact]
    public async void DownloadProcessor_NewAsset_StoresAssetAndAssetMetadata()
    {
        var mockAssetsPath = "mock/path/assets";
        var storeTuple = Utility.CreateLocalAccountDataStoreTuple(basePath: "mock/path", assetsPath: mockAssetsPath);
        var sourceTuple = Utility.CreateDataGathererMocks();
        var store = storeTuple.store;
        var mockCallbacks = sourceTuple.mockCallbacks;
        var mockSource = sourceTuple.mockAccountGatherer;
        var mockMimeType = "application/mock";
        var mockExtension = "mock";
        var mockCancellationToken = new CancellationToken();
        var mockHash = "a0a2ae9876f205fa324fd51ed56a58aeee0e71eaee6bfbb2a556";
        var mockAssetMetadata = new AssetMetadata(mockMimeType);
        using Stream mockStream = new MemoryStream();

        var completionTaskSource = new TaskCompletionSource();


        mockCallbacks.AssetToUploadAdded = new Mock<Action<AssetDiff>>().Object;
        mockCallbacks.AssetUploaded = new Mock<Action>().Object;
        mockCallbacks.AssetJobCompleted = completionTaskSource.SetResult;

        storeTuple.mimeDetectorMock.Setup(d => d.GetFileExtensionByMimeType(It.IsAny<string>())).Returns(mockExtension);
        storeTuple.fsMock.AddDirectory(mockAssetsPath);

        mockSource.Setup(s => s.GetAssetMetadata(It.IsAny<string>())).Returns(Task.FromResult(mockAssetMetadata));
        mockSource.Setup(s => s.GetAssetStream(It.IsAny<string>())).Returns(Task.FromResult(mockStream));

        store.InvokeInitDownloadProcessor(mockCancellationToken);

        store.InvokeScheduleAsset(mockHash, mockSource.Object, mockCallbacks);
        await completionTaskSource.Task;

        mockSource.Verify(s => s.GetAssetMetadata(It.Is<string>(hash => hash == mockHash)), Times.Once);
        Assert.True(storeTuple.fsMock.FileExists($"{mockAssetsPath}/{mockHash}.metadata.json"));
        mockSource.Verify(s => s.GetAssetStream(It.Is<string>(hash => hash == mockHash)));
        storeTuple.mimeDetectorMock.Verify(s => s.GetFileExtensionByMimeType(It.Is<string>(mime => mime == mockMimeType)), Times.Once);
        Assert.True(storeTuple.fsMock.FileExists($"{mockAssetsPath}/{mockHash}.{mockExtension}"));
    }

    [Fact]
    public async void DownloadProcessor_ExistingAssetWithNoMetadata_StoresAssetMetadataAndGivesExtensionToAsset()
    {
        var mockAssetsPath = "mock/path/assets";
        var storeTuple = Utility.CreateLocalAccountDataStoreTuple(basePath: "mock/path", assetsPath: mockAssetsPath);
        var sourceTuple = Utility.CreateDataGathererMocks();
        var store = storeTuple.store;
        var mockCallbacks = sourceTuple.mockCallbacks;
        var mockSource = sourceTuple.mockAccountGatherer;
        var mockExtension = "mock";
        var mockCancellationToken = new CancellationToken();
        var mockHash = "a0a2ae9876f205fa324fd51ed56a58aeee0e71eaee6bfbb2a556";
        var mockAssetMetadata = new AssetMetadata();
        var mockData = "Mock\nData";
        var mockFileData = new MockFileData(mockData);
        var mockAssetFilePath = $"{mockAssetsPath}/{mockHash}";

        var completionTaskSource = new TaskCompletionSource();


        mockCallbacks.AssetToUploadAdded = new Mock<Action<AssetDiff>>().Object;
        mockCallbacks.AssetJobCompleted = completionTaskSource.SetResult;

        storeTuple.mimeDetectorMock.Setup(d => d.MostLikelyFileExtension(It.IsAny<Stream>())).Returns(mockExtension);
        storeTuple.fsMock.AddDirectory(mockAssetsPath);
        storeTuple.fsMock.AddFile(mockAssetFilePath, mockFileData);

        mockSource.Setup(s => s.GetAssetMetadata(It.IsAny<string>())).Returns(Task.FromResult(mockAssetMetadata));

        store.InvokeInitDownloadProcessor(mockCancellationToken);

        store.InvokeScheduleAsset(mockHash, mockSource.Object, mockCallbacks);
        await completionTaskSource.Task;

        mockSource.Verify(s => s.GetAssetMetadata(It.Is<string>(hash => hash == mockHash)), Times.Once);
        Assert.True(storeTuple.fsMock.FileExists($"{mockAssetsPath}/{mockHash}.metadata.json"));
        storeTuple.mimeDetectorMock.Verify(s => s.MostLikelyFileExtension(It.IsAny<Stream>()), Times.Once);
        Assert.False(storeTuple.fsMock.FileExists(mockAssetFilePath));
        Assert.True(storeTuple.fsMock.FileExists($"{mockAssetFilePath}.{mockExtension}"));
    }

    [Fact]
    public async void DownloadProcessor_ExistingMetadataWithNoAsset_StoresAssetOnly()
    {
        var mockAssetsPath = "mock/path/assets";
        var storeTuple = Utility.CreateLocalAccountDataStoreTuple(basePath: "mock/path", assetsPath: mockAssetsPath);
        var sourceTuple = Utility.CreateDataGathererMocks();
        var store = storeTuple.store;
        var mockCallbacks = sourceTuple.mockCallbacks;
        var mockSource = sourceTuple.mockAccountGatherer;
        var mockCancellationToken = new CancellationToken();
        var mockHash = "a0a2ae9876f205fa324fd56a58aeee0e71eaee6bfbb2a556";
        var mockMimeType = "application/mock";
        var mockExtension = "mock";
        var mockFileData = Utility.CreateJsonFile(new AssetMetadata(null, mockMimeType));
        var mockAssetFilePath = $"{mockAssetsPath}/{mockHash}";
        var mockAssetMetadataFilePath = $"{mockAssetFilePath}.metadata.json";
        using Stream mockStream = new MemoryStream();

        var completionTaskSource = new TaskCompletionSource();

        mockCallbacks.AssetToUploadAdded = new Mock<Action<AssetDiff>>().Object;
        mockCallbacks.AssetJobCompleted = completionTaskSource.SetResult;

        storeTuple.mimeDetectorMock.Setup(d => d.GetFileExtensionByMimeType(It.IsAny<string>())).Returns(mockExtension);
        storeTuple.fsMock.AddDirectory(mockAssetsPath);
        storeTuple.fsMock.AddFile(mockAssetMetadataFilePath, mockFileData);

        mockSource.Setup(s => s.GetAssetStream(It.IsAny<string>())).Returns(Task.FromResult(mockStream));

        store.InvokeInitDownloadProcessor(mockCancellationToken);

        store.InvokeScheduleAsset(mockHash, mockSource.Object, mockCallbacks);
        await completionTaskSource.Task;

        mockSource.Verify(s => s.GetAssetMetadata(It.IsAny<string>()), Times.Never);
        storeTuple.mimeDetectorMock.Verify(s => s.GetFileExtensionByMimeType(It.Is<string>(mime => mime == mockMimeType)), Times.Once);
        Assert.True(storeTuple.fsMock.FileExists($"{mockAssetsPath}/{mockHash}.{mockExtension}"));
    }

    [Fact]
    public async void DownloadProcessor_ExistingAssetAndMetadata_CompletesJob()
    {
        var mockAssetsPath = "mock/path/assets";
        var storeTuple = Utility.CreateLocalAccountDataStoreTuple(basePath: "mock/path", assetsPath: mockAssetsPath);
        var sourceTuple = Utility.CreateDataGathererMocks();
        var store = storeTuple.store;
        var mockCallbacks = sourceTuple.mockCallbacks;
        var mockSource = sourceTuple.mockAccountGatherer;
        var mockCancellationToken = new CancellationToken();
        var mockHash = "a0a2ae9876f205fa324fd51ed56a58aeee0e71eaee6bfbb2a556";
        var mockFileData = Utility.CreateJsonFile(new AssetMetadata(null, string.Empty));
        var mockAssetFilePath = $"{mockAssetsPath}/{mockHash}";
        var mockAssetMetadataFilePath = $"{mockAssetFilePath}.metadata.json";

        var completionTaskSource = new TaskCompletionSource();

        mockCallbacks.AssetToUploadAdded = new Mock<Action<AssetDiff>>().Object;
        mockCallbacks.AssetJobCompleted = completionTaskSource.SetResult;

        storeTuple.fsMock.AddDirectory(mockAssetsPath);
        storeTuple.fsMock.AddFile(mockAssetMetadataFilePath, mockFileData);
        storeTuple.fsMock.AddEmptyFile($"{mockAssetFilePath}.mock");

        store.InvokeInitDownloadProcessor(mockCancellationToken);

        store.InvokeScheduleAsset(mockHash, mockSource.Object, mockCallbacks);
        await completionTaskSource.Task;

        mockSource.Verify(s => s.GetAssetMetadata(It.IsAny<string>()), Times.Never);
        mockSource.Verify(s => s.GetAssetStream(It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async void DownloadProcessor_ExceptionThrown_CompletesJob()
    {
        var mockAssetsPath = "mock/path/assets";
        var storeTuple = Utility.CreateLocalAccountDataStoreTuple(basePath: "mock/path", assetsPath: mockAssetsPath);
        var sourceTuple = Utility.CreateDataGathererMocks();
        var store = storeTuple.store;
        var mockCallbacks = sourceTuple.mockCallbacks;
        var mockSource = sourceTuple.mockAccountGatherer;
        var mockCancellationToken = new CancellationToken();
        var mockHash = "a0a2ae9876f205fa324fd51ed56a58aeee0e71eaee6bfbb2a556";

        var completionTaskSource = new TaskCompletionSource();

        mockCallbacks.AssetToUploadAdded = new Mock<Action<AssetDiff>>().Object;
        mockCallbacks.AssetJobCompleted = completionTaskSource.SetResult;
        mockSource.Setup(s => s.GetAssetMetadata(It.IsAny<string>())).Throws(() => new Exception());

        store.InvokeInitDownloadProcessor(mockCancellationToken);
        store.InvokeScheduleAsset(mockHash, mockSource.Object, mockCallbacks);
        await completionTaskSource.Task;

        Assert.False(storeTuple.fsMock.FileExists($"{mockAssetsPath}/{mockHash}.metadata.json"));
        mockSource.Verify(s => s.GetAssetStream(It.IsAny<string>()), Times.Never);
    }
}
static class LocalAccountDataStoreTestExtensionMethods
{
    internal static void InvokeInitDownloadProcessor(this LocalAccountDataStore store, CancellationToken token)
    {
        var storeScheduleAssetMethod = typeof(LocalAccountDataStore).GetMethod("InitDownloadProcessor", BindingFlags.NonPublic | BindingFlags.Instance);
        storeScheduleAssetMethod!.Invoke(store, new object[] { token });
    }
    internal static void InvokeScheduleAsset(this LocalAccountDataStore store, string hash, IAccountDataGatherer source, RecordStatusCallbacks callbacks)
    {
        var storeScheduleAssetMethod = typeof(LocalAccountDataStore).GetMethod("ScheduleAsset", BindingFlags.NonPublic | BindingFlags.Instance);
        storeScheduleAssetMethod!.Invoke(store, new object[] { hash, source, callbacks });
    }
}
