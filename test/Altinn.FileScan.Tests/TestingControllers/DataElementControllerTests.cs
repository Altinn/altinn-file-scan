using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;

using Altinn.Common.AccessToken.Services;
using Altinn.FileScan.Controllers;
using Altinn.FileScan.Models;
using Altinn.FileScan.Services.Interfaces;
using Altinn.FileScan.Tests.Mocks;
using Altinn.FileScan.Tests.Mocks.Authentication;
using Altinn.FileScan.Tests.Utils;

using AltinnCore.Authentication.JwtCookie;

using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

using Moq;

using Xunit;

namespace Altinn.FileScan.Tests.TestingControllers
{
    /// <summary>
    /// Represents a collection of tests of the <see cref="DataElementController"/>.
    /// </summary>
    public class DataElementControllerTests : IClassFixture<WebApplicationFactory<DataElementController>>
    {
        private const string BasePath = "/filescan/api/v1";

        private readonly WebApplicationFactory<DataElementController> _factory;
        private readonly string serializedDataElement;

        /// <summary>
        /// Initializes a new instance of the <see cref="DataElementControllerTests"/> class with the given <see cref="WebApplicationFactory{DataElementController}"/>.
        /// </summary>
        /// <param name="factory">The <see cref="WebApplicationFactory{TPushController}"/> to use when setting up the test server.</param>
        public DataElementControllerTests(WebApplicationFactory<DataElementController> factory)
        {
            _factory = factory;
            serializedDataElement = "{" +
                "\"id\": \"11f7c994-6681-47a1-9626-fcf6c27308a5\"," +
                "\"instanceGuid\": \"649388f0-a2c0-4774-bd11-c870223ed819\"," +
                "\"dataType\": \"default\"," +
                "\"contentType\": \"text/plain; charset=utf-8\"," +
                "\"blobStoragePath\": \"tdd/endring-av-navn/649388f0-a2c0-4774-bd11-c870223ed819/data/11f7c994-6681-47a1-9626-fcf6c27308a5\"," +
                "\"size\": 19," +
                "\"locked\": false," +
                "\"created\": \"2020-05-11T17:09:28.4621953Z\"," +
                "\"lastChanged\": \"2020-05-11T17:09:28.4621953Z\"" +
                "}";
        }

        /// <summary>
        /// Scenario:
        ///   Post a request to ScanDataElement endpoint include platform access token
        /// Expected result:
        ///   Returns HttpStatus Ok.
        /// Success criteria:
        ///   The response has correct status code.
        /// </summary>
        [Fact]
        public async void Post_ScanDataElement_PlatformAccessTokenIncluded()
        {
            // Arrange
            string requestUri = $"{BasePath}/dataelement";
            var dataElementMock = new Mock<IDataElement>();
            dataElementMock
                .Setup(de => de.Scan(It.IsAny<DataElementScanRequest>()));

            HttpClient client = GetTestClient(dataElementMock.Object);

            HttpRequestMessage httpRequestMessage = new(HttpMethod.Post, requestUri)
            {
                Content = new StringContent(serializedDataElement, Encoding.UTF8, "application/json")
            };

            httpRequestMessage.Headers.Add("PlatformAccessToken", PrincipalUtil.GetAccessToken("platform", "file-scan"));

            // Act
            HttpResponseMessage response = await client.SendAsync(httpRequestMessage);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        /// <summary>
        /// Scenario:
        ///   Post a request to ScanDataElement endpoint ommit platform access token
        /// Expected result:
        ///   Returns HttpStatus Forbidden.
        /// Success criteria:
        ///   The response has correct status code.
        /// </summary>
        [Fact]
        public async void Post_ScanDataElement_PlatformAccessTokenOmmited_BearerIncluded()
        {
            // Arrange
            string requestUri = $"{BasePath}/dataelement";
            HttpClient client = GetTestClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", PrincipalUtil.GetToken(1));
            HttpRequestMessage httpRequestMessage = new(HttpMethod.Post, requestUri)
            {
                Content = new StringContent(serializedDataElement, Encoding.UTF8, "application/json")
            };

            // Act
            HttpResponseMessage response = await client.SendAsync(httpRequestMessage);

            // Assert
            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        }

        /// <summary>
        /// Scenario:
        ///   Post a request to ScanDataElement endpoint ommit platform access token
        /// Expected result:
        ///   Returns HttpStatus Forbidden.
        /// Success criteria:
        ///   The response has correct status code.
        /// </summary>
        [Fact]
        public async void Post_ScanDataElement_PlatformAccessTokenOmmited_BearerTokenPresent()
        {
            // Arrange
            string requestUri = $"{BasePath}/dataelement";
            HttpClient client = GetTestClient();
            HttpRequestMessage httpRequestMessage = new(HttpMethod.Post, requestUri)
            {
                Content = new StringContent(serializedDataElement, Encoding.UTF8, "application/json")
            };

            // Act
            HttpResponseMessage response = await client.SendAsync(httpRequestMessage);

            // Assert
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        private HttpClient GetTestClient(IDataElement dataElementMock = null)
        {
            dataElementMock ??= new Mock<IDataElement>().Object;

            HttpClient client = _factory.WithWebHostBuilder(builder =>
            {
                builder.ConfigureTestServices(services =>
                {
                    // Set up mock authentication so that not well known endpoint is used
                    services.AddSingleton<IPostConfigureOptions<JwtCookieOptions>, JwtCookiePostConfigureOptionsStub>();
                    services.AddSingleton<ISigningKeysResolver, SigningKeyResolverMock>();

                    services.AddSingleton(dataElementMock);
                });
            }).CreateClient();

            return client;
        }
    }
}
