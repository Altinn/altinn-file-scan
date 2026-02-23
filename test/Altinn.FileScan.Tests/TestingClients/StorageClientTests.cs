#nullable enable
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Altinn.FileScan.Clients;
using Altinn.FileScan.Configuration;
using Altinn.FileScan.Exceptions;
using Altinn.FileScan.Services.Interfaces;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;

namespace Altinn.FileScan.Tests.TestingClients;

public class StorageClientTests
{
    [Fact]
    public async Task DataElementExists_HappyPath_ElementExists()
    {
        // Arrange
        const string instanceId = "someInstanceId";
        const string dataElementId = "someDataElementId";
        const string accessToken = "someToken";
        const string requestContent = "true";

        Mock<IAccessToken> accessTokenMock = new Mock<IAccessToken>();
        Mock<HttpClient> httpClientMock = new Mock<HttpClient>();
        accessTokenMock
            .Setup(s => s.Generate())
            .ReturnsAsync(accessToken);
        httpClientMock
            .Setup(s => s.SendAsync(It.IsAny<HttpRequestMessage>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new HttpResponseMessage { StatusCode = HttpStatusCode.OK, Content = new StringContent(requestContent) });

        StorageClient testService = SetupTestService(httpClientMock.Object, accessTokenMock.Object, null);

        // Act
        bool result = await testService.DataElementExists(instanceId, dataElementId);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public async Task DataElementExists_HappyPath_ElementDoesNotExist()
    {
        // Arrange
        const string instanceId = "someInstanceId";
        const string dataElementId = "someDataElementId";
        const string accessToken = "someToken";
        const string requestContent = "false";

        Mock<IAccessToken> accessTokenMock = new Mock<IAccessToken>();
        Mock<HttpClient> httpClientMock = new Mock<HttpClient>();
        accessTokenMock
            .Setup(s => s.Generate())
            .ReturnsAsync(accessToken);
        httpClientMock
            .Setup(s => s.SendAsync(It.IsAny<HttpRequestMessage>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new HttpResponseMessage { StatusCode = HttpStatusCode.OK, Content = new StringContent(requestContent) });

        StorageClient testService = SetupTestService(httpClientMock.Object, accessTokenMock.Object, null);

        // Act
        bool result = await testService.DataElementExists(instanceId, dataElementId);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task DataElementExists_ThrowsPlatformHttpException()
    {
        // Arrange
        const string instanceId = "someInstanceId";
        const string dataElementId = "someDataElementId";
        const string accessToken = "someToken";

        Mock<IAccessToken> accessTokenMock = new Mock<IAccessToken>();
        Mock<HttpClient> httpClientMock = new Mock<HttpClient>();
        accessTokenMock
            .Setup(s => s.Generate())
            .ReturnsAsync(accessToken);
        httpClientMock
            .Setup(s => s.SendAsync(It.IsAny<HttpRequestMessage>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new HttpResponseMessage { StatusCode = HttpStatusCode.BadRequest });

        StorageClient testService = SetupTestService(httpClientMock.Object, accessTokenMock.Object, null);

        // Act
        Task Act() => testService.DataElementExists(instanceId, dataElementId);

        // Assert
        PlatformHttpException exception = await Assert.ThrowsAsync<PlatformHttpException>(Act);
        Assert.Equal("Unexpected response from StorageClient when checking if data element exists.", exception.Message);
    }

    private static StorageClient SetupTestService(HttpClient? httpClient, IAccessToken? accessToken, IOptions<PlatformSettings>? platformSettings)
    {
        httpClient ??= new Mock<HttpClient>().Object;
        accessToken ??= new Mock<IAccessToken>().Object;
        platformSettings ??= Options.Create(new PlatformSettings { ApiStorageEndpoint = "https://example.com/" });
        return new StorageClient(httpClient, accessToken, platformSettings);
    }
}
