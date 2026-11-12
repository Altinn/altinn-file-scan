using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Altinn.FileScan.Clients;
using Altinn.FileScan.Configuration;
using Altinn.FileScan.Exceptions;
using Altinn.FileScan.Models;
using Altinn.FileScan.Services.Interfaces;
using Altinn.Platform.Storage.Interface.Enums;
using Altinn.Platform.Storage.Interface.Models;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;

namespace Altinn.FileScan.Tests.TestingClients;

public class StorageClientTests
{
    private const string _readUrl = "https://account.blob.core.windows.net/c/b?sig=secret";
    private static readonly byte[] _blobContent = [1, 2, 3];

    /// <summary>
    /// Scenario: Storage says the blob version awaits its scan.
    /// Expected result: The blob is downloaded through the read URL from Storage.
    /// Success criteria: Storage is asked with the platform access token, and the blob is read with one GET of
    /// the read URL that carries no credential of FileScan's own.
    /// </summary>
    [Fact]
    public async Task GetBlobToScan_Pending_DownloadsBlobThroughReadUrlWithoutCredential()
    {
        // Arrange
        StubHandler storage = new(_ =>
            Json($$"""{"status":"pending","readUrl":"{{_readUrl}}","expiresAt":"2026-11-12T10:05:00+00:00"}""")
        );
        StubHandler blobs = new(_ => Blob(HttpStatusCode.OK));
        StorageClient sut = SetupTestService(storage, blobs, "file-scan-token");

        // Act
        using BlobToScan blob = await sut.GetBlobToScan("1337/instance", "element", "version");

        // Assert
        Assert.Equal(FileScanTargetStatus.Pending, blob.Status);
        using MemoryStream content = new();
        await blob.Content!.CopyToAsync(content);
        Assert.Equal(_blobContent, content.ToArray());

        HttpRequestMessage storageRequest = Assert.Single(storage.Requests);
        Assert.Equal(HttpMethod.Get, storageRequest.Method);
        Assert.Equal(
            "https://example.com/instances/1337/instance/dataelements/element/blobversions/version/filescan",
            storageRequest.RequestUri!.AbsoluteUri
        );
        Assert.Equal(["file-scan-token"], storageRequest.Headers.GetValues("PlatformAccessToken"));

        HttpRequestMessage blobRequest = Assert.Single(blobs.Requests);
        Assert.Equal(HttpMethod.Get, blobRequest.Method);
        Assert.Equal(new Uri(_readUrl), blobRequest.RequestUri);
        Assert.Null(blobRequest.Headers.Authorization);
        Assert.False(blobRequest.Headers.Contains("PlatformAccessToken"));
    }

    /// <summary>
    /// Scenario: Storage says the blob version doesn't await its scan, with its status as a camel-cased string.
    /// Expected result: The status is returned and nothing is downloaded.
    /// Success criteria: Each string maps to its status, and the blob client is never called.
    /// </summary>
    [Theory]
    [InlineData("superseded", FileScanTargetStatus.Superseded)]
    [InlineData("notPending", FileScanTargetStatus.NotPending)]
    [InlineData("deleted", FileScanTargetStatus.Deleted)]
    public async Task GetBlobToScan_NotAwaitingScan_ReturnsStatusWithoutDownload(
        string status,
        FileScanTargetStatus expected
    )
    {
        // Arrange
        StubHandler blobs = new(_ => Blob(HttpStatusCode.OK));
        StorageClient sut = SetupTestService(new StubHandler(_ => Json($$"""{"status":"{{status}}"}""")), blobs);

        // Act
        using BlobToScan blob = await sut.GetBlobToScan("1337/instance", "element", "version");

        // Assert
        Assert.Equal(expected, blob.Status);
        Assert.Null(blob.Content);
        Assert.Empty(blobs.Requests);
    }

    /// <summary>
    /// Scenario: Storage doesn't give a decision, for example because the endpoint doesn't exist yet.
    /// Expected result: This is an error, never a reason to skip the scan.
    /// Success criteria: A PlatformHttpException is thrown and nothing is downloaded.
    /// </summary>
    [Theory]
    [InlineData(HttpStatusCode.NotFound)]
    [InlineData(HttpStatusCode.Forbidden)]
    [InlineData(HttpStatusCode.InternalServerError)]
    public async Task GetBlobToScan_StorageError_ThrowsPlatformHttpException(HttpStatusCode statusCode)
    {
        // Arrange
        StubHandler blobs = new(_ => Blob(HttpStatusCode.OK));
        StorageClient sut = SetupTestService(new StubHandler(_ => new HttpResponseMessage(statusCode)), blobs);

        // Act
        Task Act() => sut.GetBlobToScan("1337/instance", "element", "version");

        // Assert
        PlatformHttpException exception = await Assert.ThrowsAsync<PlatformHttpException>(Act);
        Assert.Equal(statusCode, exception.Response.StatusCode);
        Assert.Empty(blobs.Requests);
    }

    /// <summary>
    /// Scenario: Storage answers 200 with a body that doesn't follow the contract.
    /// Expected result: This is an error, never a reason to skip or to scan.
    /// Success criteria: A PlatformHttpException is thrown for a missing status, a numeric status, an unknown
    /// status, a pending status without a read URL, and a body that isn't JSON.
    /// </summary>
    [Theory]
    [InlineData("{}")]
    [InlineData("""{"status":0}""")]
    [InlineData("""{"status":"archived"}""")]
    [InlineData("""{"status":"pending"}""")]
    [InlineData("not json")]
    public async Task GetBlobToScan_UnexpectedBody_ThrowsPlatformHttpException(string body)
    {
        // Arrange
        StubHandler blobs = new(_ => Blob(HttpStatusCode.OK));
        StorageClient sut = SetupTestService(new StubHandler(_ => Json(body)), blobs);

        // Act
        Task Act() => sut.GetBlobToScan("1337/instance", "element", "version");

        // Assert
        await Assert.ThrowsAsync<PlatformHttpException>(Act);
        Assert.Empty(blobs.Requests);
    }

    /// <summary>
    /// Scenario: The read URL is refused, or there is no blob behind it, for example because it expired or the
    /// data element was deleted after Storage issued it.
    /// Expected result: The request fails, so it is retried and Storage decides again.
    /// Success criteria: A PlatformHttpException with the download's status is thrown.
    /// </summary>
    [Theory]
    [InlineData(HttpStatusCode.NotFound)]
    [InlineData(HttpStatusCode.Forbidden)]
    public async Task GetBlobToScan_DownloadFails_ThrowsPlatformHttpException(HttpStatusCode statusCode)
    {
        // Arrange
        StorageClient sut = SetupTestService(
            new StubHandler(_ => Json($$"""{"status":"pending","readUrl":"{{_readUrl}}"}""")),
            new StubHandler(_ => Blob(statusCode))
        );

        // Act
        Task Act() => sut.GetBlobToScan("1337/instance", "element", "version");

        // Assert
        PlatformHttpException exception = await Assert.ThrowsAsync<PlatformHttpException>(Act);
        Assert.Equal(statusCode, exception.Response.StatusCode);
    }

    public static TheoryData<FileScanStatusOutcome> FileScanStatusOutcomes =>
        new(Enum.GetValues<FileScanStatusOutcome>());

    /// <summary>
    /// Scenario: Storage answers the file scan status with its outcome as a camel-cased string.
    /// Expected result: Every outcome Storage can send is understood.
    /// Success criteria: The camel-cased name of each outcome maps to that outcome.
    /// </summary>
    [Theory]
    [MemberData(nameof(FileScanStatusOutcomes))]
    public async Task PatchFileScanStatus_EveryOutcome_ReadsCamelCaseString(FileScanStatusOutcome outcome)
    {
        // Arrange
        StubHandler storage = new(_ =>
            Json($$"""{"status":"{{JsonNamingPolicy.CamelCase.ConvertName(outcome.ToString())}}"}""")
        );
        StorageClient sut = SetupTestService(storage, accessToken: "file-scan-token");

        // Act
        FileScanStatusOutcome result = await sut.PatchFileScanStatus(
            "1337/instance",
            "element",
            new FileScanStatus { FileScanResult = FileScanResult.Clean, BlobVersionId = "version" }
        );

        // Assert
        Assert.Equal(outcome, result);
        HttpRequestMessage request = Assert.Single(storage.Requests);
        Assert.Equal(HttpMethod.Put, request.Method);
        Assert.Equal(
            "https://example.com/instances/1337/instance/dataelements/element/filescanstatus",
            request.RequestUri!.AbsoluteUri
        );
        Assert.Equal(["file-scan-token"], request.Headers.GetValues("PlatformAccessToken"));
    }

    [Theory]
    [InlineData(HttpStatusCode.BadRequest, "")]
    [InlineData(HttpStatusCode.OK, "")]
    [InlineData(HttpStatusCode.OK, """{"status":1}""")]
    public async Task PatchFileScanStatus_UnexpectedResponse_ThrowsPlatformHttpException(
        HttpStatusCode statusCode,
        string body
    )
    {
        // Arrange
        StorageClient sut = SetupTestService(
            new StubHandler(_ => new HttpResponseMessage(statusCode) { Content = new StringContent(body) })
        );

        // Act
        Task Act() =>
            sut.PatchFileScanStatus(
                "1337/instance",
                "element",
                new FileScanStatus { FileScanResult = FileScanResult.Clean }
            );

        // Assert
        await Assert.ThrowsAsync<PlatformHttpException>(Act);
    }

    private static HttpResponseMessage Json(string body) =>
        new(HttpStatusCode.OK) { Content = new StringContent(body) };

    private static HttpResponseMessage Blob(HttpStatusCode statusCode) =>
        new(statusCode) { Content = new ByteArrayContent(_blobContent) };

    private static StorageClient SetupTestService(
        StubHandler storage,
        StubHandler? blobs = null,
        string accessToken = "token"
    )
    {
        Mock<IAccessToken> accessTokenMock = new();
        accessTokenMock.Setup(s => s.Generate()).ReturnsAsync(accessToken);
        blobs ??= new StubHandler(_ => throw new InvalidOperationException("No blob download expected."));
        Mock<IHttpClientFactory> httpClientFactory = new();
        httpClientFactory
            .Setup(f => f.CreateClient(StorageClient.BlobDownloadHttpClientName))
            .Returns(() => new HttpClient(blobs, disposeHandler: false));
        return new StorageClient(
            new HttpClient(storage),
            httpClientFactory.Object,
            accessTokenMock.Object,
            Options.Create(new PlatformSettings { ApiStorageEndpoint = "https://example.com/" })
        );
    }

    private sealed class StubHandler(Func<HttpRequestMessage, HttpResponseMessage> respond) : HttpMessageHandler
    {
        public List<HttpRequestMessage> Requests { get; } = [];

        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken
        )
        {
            Requests.Add(request);
            return Task.FromResult(respond(request));
        }
    }
}
