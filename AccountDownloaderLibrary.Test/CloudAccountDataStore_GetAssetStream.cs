using System.Net;
using AccountDownloaderLibrary.Implementations;
using SoloX.CodeQuality.Test.Helpers.Http;

namespace AccountDownloaderLibrary.Test;

public class CloudAccountDataStore_GetAssetStream
{ 
    [Fact]
    public async void GetAssetStream_HashValue_ReturnsStreamForCloudAsset()
    {
        var mockHash = "20a2ae9876f205fa324fd51ed56a58aeee0e71eaee6bfbb2a556";
        var mockBytes = new byte[35];
        Random.Shared.NextBytes(mockBytes);

        var mockHttpClient = Utility.StartNeosAssetsWebClientBuilder()
            .WithRequest($"assets/{mockHash}")
            .Responding((_) => new HttpResponseMessage
            {
                Content =  new StreamContent(new MemoryStream(mockBytes))
            })
            .Build();

        var tuple = Utility.CreateCloudAccountDataStoreTuple(mockHttpClient);
        using var resultStream = await tuple.store.GetAssetStream(mockHash);
        using var binaryReader = new BinaryReader(resultStream);

        Assert.Equal(mockBytes, binaryReader.ReadBytes(mockBytes.Length));
    }

    [Theory]
    [InlineData(HttpStatusCode.Forbidden)]
    [InlineData(HttpStatusCode.NotFound)]
    [InlineData(HttpStatusCode.Unauthorized)]
    [InlineData(HttpStatusCode.BadRequest)]
    [InlineData(HttpStatusCode.TooManyRequests)]
    [InlineData(HttpStatusCode.InternalServerError)]
    public async void GetAssetStream_BadHash_ThrowsCloudXAssetResponseError(HttpStatusCode statusCode)
    {
        var mockHash = "20a2ae9876f205fa324fd51ed56a58aeee0e71eaee6bfbb2a556";
        var mockBytes = new byte[35];
        Random.Shared.NextBytes(mockBytes);

        var mockHttpClient = Utility.StartNeosAssetsWebClientBuilder()
            .WithRequest($"assets/{mockHash}")
            .RespondingStatus(statusCode)
            .Build();

        var tuple = Utility.CreateCloudAccountDataStoreTuple(mockHttpClient);
        await Assert.ThrowsAsync<CloudXAssetResponseErrorException>(async () =>
        {
            await tuple.store.GetAssetStream(mockHash);
        });
    }
}
