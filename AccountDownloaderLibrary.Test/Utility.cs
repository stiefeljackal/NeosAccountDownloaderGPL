using CloudX.Shared;
using Moq;
using System.IO.Abstractions.TestingHelpers;
using SoloX.CodeQuality.Test.Helpers.Http;
using AccountDownloaderLibrary.Mime.Interfaces;
using AccountDownloaderLibrary.Mime;
using System.Text.Json;

namespace AccountDownloaderLibrary.Test;

internal static class Utility
{
    internal static (CloudAccountDataStore store, Mock<CloudXInterface> cloudXInterfaceMock, Mock<AccountDownloadConfig> configMock) CreateCloudAccountDataStoreTuple(HttpClient? assetsClient = null, HttpClient? cloudXApiClient = null)
    {
        if (assetsClient == null)
        {
            assetsClient = StartNeosAssetsWebClientBuilder().Build();
        }

        Mock<CloudXInterface> cloudXInterfaceMock = new (MockBehavior.Loose, "uid:abc123", "CloudX", "0.0.0.0", false);
        Mock<AccountDownloadConfig> configMock = new();
        CloudAccountDataStore store = new (cloudXInterfaceMock.Object, assetsClient, configMock.Object);

        if (cloudXApiClient != null)
        {
            typeof(CloudXInterface).GetProperty("HttpClient")?.SetValue(cloudXInterfaceMock.Object, cloudXApiClient);
        }

        return (store, cloudXInterfaceMock, configMock);
    }

    internal static (LocalAccountDataStore store, MockFileSystem fsMock, Mock<IMimeDetector> mimeDetectorMock, Mock<AccountDownloadConfig> configMock) CreateLocalAccountDataStoreTuple(string userId = "", string basePath = "", string assetsPath = "")
    {
        MockFileSystem fileSystemMock = new();
        Mock<AccountDownloadConfig> configMock = new();
        Mock<IMimeDetector> mimeDetectorMock = new();
        LocalAccountDataStore store = new (userId, basePath, assetsPath, fileSystemMock, mimeDetectorMock.Object, configMock.Object);

        return (store, fileSystemMock, mimeDetectorMock, configMock);
    }

    internal static (MimeDetector mimeDetector, MockFileSystem mockFs) CreateMimeDetectorTuple()
    {
        MockFileSystem fileSystemMock = new();
        MimeDetector mimeDetector = new(fileSystemMock);

        return (mimeDetector, fileSystemMock);
    }

    internal static (Mock<IAccountDataGatherer> mockAccountGatherer, RecordStatusCallbacks mockCallbacks) CreateDataGathererMocks()
    {
        var mockSource = new Mock<IAccountDataGatherer>(MockBehavior.Loose);
        var mockCallbacks = new RecordStatusCallbacks();

        return (mockSource, mockCallbacks);
    }

    internal static MockFileData CreateJsonFile<T>(T serializeable) where T : new() =>
        new MockFileData(JsonSerializer.SerializeToUtf8Bytes(serializeable, typeof(T)));

    internal static IHttpClientRequestMockBuilder StartNeosAssetsWebClientBuilder() =>
        new HttpClientMockBuilder()
            .WithBaseAddress(new Uri(@"https://assets.neos.com/"));

    internal static IHttpClientRequestMockBuilder StartNeosApiWebClientBuilder() =>
        new HttpClientMockBuilder()
            .WithBaseAddress(new Uri(@"https://api.neos.com/"));
}
