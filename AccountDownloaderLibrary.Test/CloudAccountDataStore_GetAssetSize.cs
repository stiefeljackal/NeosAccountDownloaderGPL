using CloudX.Shared;
using Moq;
using SoloX.CodeQuality.Test.Helpers.Http;
using System.IO.Abstractions.TestingHelpers;
using System.Net;

namespace AccountDownloaderLibrary.Test;

public class CloudAccountDataStore_GetAssetSize
{
    public CloudAccountDataStore_GetAssetSize()
    {
        CloudXInterface.DEFAULT_RETRIES = 0;
    }

    [Fact]
    public async void GetAssetSize_HashValue_ReturnsTheAssetSize()
    {
        var mockHash = "20a2ae9876f205fa324fd51ed56a58aeee0e71eaee6bfbb2a556";
        var mockSize = Random.Shared.NextInt64();

        var mockCloudXApiClient = Utility.StartNeosApiWebClientBuilder()
            .WithRequest($"api/assets/{mockHash}")
            .RespondingJsonContent((_) => new AssetInfo
            {
                Bytes = mockSize
            })
            .Build();


        var tuple = Utility.CreateCloudAccountDataStoreTuple(cloudXApiClient: mockCloudXApiClient);

        var assetSize = await tuple.store.GetAssetSize(mockHash);

        Assert.Equal(mockSize, assetSize);
    }

    [Theory]
    [InlineData(HttpStatusCode.OK)]
    [InlineData(HttpStatusCode.NoContent)]
    [InlineData(HttpStatusCode.Accepted)]
    [InlineData(HttpStatusCode.Forbidden)]
    [InlineData(HttpStatusCode.NotFound)]
    [InlineData(HttpStatusCode.Unauthorized)]
    [InlineData(HttpStatusCode.BadRequest)]
    [InlineData(HttpStatusCode.TooManyRequests)]
    [InlineData(HttpStatusCode.InternalServerError)]
    public async void GetAssetSize_BadHash_ReturnsZeroForBytes(HttpStatusCode statusCode)
    {
        var mockHash = "20a2ae9876f205fa324fd51ed56a58aeee0e71eaee6bfbb2a556";
        var mockCloudXApiClient = Utility.StartNeosApiWebClientBuilder()
            .WithRequest($"api/assets/{mockHash}")
            .RespondingStatus(statusCode)
            .Build();


        var tuple = Utility.CreateCloudAccountDataStoreTuple(cloudXApiClient: mockCloudXApiClient);

        var assetSize = await tuple.store.GetAssetSize(mockHash);

        Assert.Equal(0, assetSize);
    }
}
