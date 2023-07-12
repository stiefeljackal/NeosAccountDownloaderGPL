using CloudX.Shared;
using Moq;
using SoloX.CodeQuality.Test.Helpers.Http;
using System.Net;

namespace AccountDownloaderLibrary.Test;

public class CloudAccountDataStore_GetAssetMetadata
{
    public CloudAccountDataStore_GetAssetMetadata()
    {
        CloudXInterface.DEFAULT_RETRIES = 0;
    }

    [Theory]
    [InlineData("application/mock")]
    [InlineData("")]
    public async void GetAssetMetadata_HashValue_ReturnsAssetMetadata(string mockMimeType)
    {
        var mockHash = "20a2ae9876f205fa324fd51ed56a58aeee0e71eaee6bfbb2a556";
        var mockByteSize = Random.Shared.Next();

        var mockCloudXApiClient = Utility.StartNeosApiWebClientBuilder()
            .WithRequest($"api/assets/{mockHash}")
            .RespondingJsonContent((_) => new AssetInfo
            {
                Bytes = mockByteSize
            })
            .WithRequest($"api/assets/{mockHash}/mime")
            .RespondingJsonContent((_) => mockMimeType)
            .Build();

        var tuple = Utility.CreateCloudAccountDataStoreTuple(cloudXApiClient: mockCloudXApiClient);

        var assetData = await tuple.store.GetAssetMetadata(mockHash);

        Assert.Equal(mockByteSize, assetData.Size);
        Assert.Equal(mockMimeType, assetData.MimeType);
        Assert.Equal(new Uri($"https://assets.neos.com/assets/{mockHash}"), assetData.Url);
    }

    [Fact]
    public async void GetAssetMetadata_NullOrEmptyAssetData_ReturnsAssetMetadataWithOnlyUri()
    {
        var mockHash = "20a2ae9876f205fa324fd51ed56a58aeee0e71eaee6bfbb2a556";
        var mockCloudXApiClient = Utility.StartNeosApiWebClientBuilder()
            .WithRequest($"api/assets/{mockHash}")
            .RespondingStatus(HttpStatusCode.OK)
            .WithRequest($"api/assets/{mockHash}/mime")
            .RespondingStatus(HttpStatusCode.NoContent)
            .Build();

        var tuple = Utility.CreateCloudAccountDataStoreTuple(cloudXApiClient: mockCloudXApiClient);

        var assetData = await tuple.store.GetAssetMetadata(mockHash);

        Assert.Equal(0, assetData.Size);
        Assert.Equal("", assetData.MimeType);
        Assert.Equal(new Uri($"https://assets.neos.com/assets/{mockHash}"), assetData.Url);
    }

    [Fact]
    public async void GetAssetMetadata_HashWithNoMimeEndpoint_ReturnsAssetMetadata()
    {
        var mockHash = "20a2ae9876f205fa324fd51ed56a58aeee0e71eaee6bfbb2a556";
        var mockMimeType = "application/mock";
        var mockBytes = new byte[35];
        Random.Shared.NextBytes(mockBytes);
        using var mockStream = new MemoryStream(mockBytes);

        var mockCloudXApiClient = Utility.StartNeosApiWebClientBuilder()
            .WithRequest($"api/assets/{mockHash}")
            .RespondingJsonContent((_) => new AssetInfo
            {
                Bytes = mockBytes.LongLength
            })
            .WithRequest($"api/assets/{mockHash}/mime")
            .RespondingStatus(HttpStatusCode.NotFound)
            .Build();

        var mockHttpClient = Utility.StartNeosAssetsWebClientBuilder()
            .WithRequest($"assets/{mockHash}")
            .Responding((_) => {
                var response = new HttpResponseMessage()
                {
                    Content = new StreamContent(mockStream)
                };
                if (mockMimeType != null)
                {
                    response.Content.Headers.Add("Content-Type", mockMimeType);
                }
                return response;
            })
            .Build();

        var tuple = Utility.CreateCloudAccountDataStoreTuple(mockHttpClient, mockCloudXApiClient);
        var assetData = await tuple.store.GetAssetMetadata(mockHash);

        Assert.Equal(mockBytes.LongLength, assetData.Size);
        Assert.Equal(mockMimeType ?? "", assetData.MimeType);
        Assert.Equal(new Uri($"https://assets.neos.com/assets/{mockHash}"), assetData.Url);
    }

    [Theory]
    [InlineData(HttpStatusCode.NoContent)]
    [InlineData(HttpStatusCode.Forbidden)]
    [InlineData(HttpStatusCode.NotFound)]
    [InlineData(HttpStatusCode.Unauthorized)]
    [InlineData(HttpStatusCode.BadRequest)]
    [InlineData(HttpStatusCode.TooManyRequests)]
    [InlineData(HttpStatusCode.InternalServerError)]
    public async void GetAssetMetadata_NullOrEmptyAsset_ReturnsAssetMetadataWithOnlyUri(HttpStatusCode statusCode)
    {
        var mockHash = "20a2ae9876f205fa324fd51ed56a58aeee0e71eaee6bfbb2a556";

        var mockCloudXApiClient = Utility.StartNeosApiWebClientBuilder()
            .WithRequest($"api/assets/{mockHash}")
            .RespondingStatus(statusCode)
            .WithRequest($"api/assets/{mockHash}/mime")
            .RespondingStatus(HttpStatusCode.NotFound)
            .Build();

        var mockHttpClient = Utility.StartNeosAssetsWebClientBuilder()
            .WithRequest($"assets/{mockHash}")
            .RespondingStatus(statusCode)
            .Build();

        var tuple = Utility.CreateCloudAccountDataStoreTuple(mockHttpClient, mockCloudXApiClient);
        var assetData = await tuple.store.GetAssetMetadata(mockHash);

        Assert.Equal(0, assetData.Size);
        Assert.Equal("", assetData.MimeType);
        Assert.Equal(new Uri($"https://assets.neos.com/assets/{mockHash}"), assetData.Url);
    }
}
