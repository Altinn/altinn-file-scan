using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using Altinn.Common.AccessTokenClient.Configuration;
using Altinn.Common.AccessTokenClient.Services;
using Altinn.FileScan.Services;
using Altinn.FileScan.Services.Interfaces;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Moq;
using Xunit;

namespace Altinn.FileScan.Tests.TestingServices;

public class AccessTokenServiceTests
{
    /// <summary>
    /// Scenario: The service generates access tokens for calls to Storage.
    /// Expected result: Tokens are issued by platform for the app file-scan, which Storage requires for the blob
    /// read URL endpoint.
    /// Success criteria: The token has issuer platform and the urn:altinn:app claim file-scan, is signed with the
    /// platform certificate, and the certificate is read from Key Vault once.
    /// </summary>
    [Fact]
    public async Task Generate_IssuesPlatformTokenForFileScanAndReadsCertificateOnce()
    {
        // Arrange
        using X509Certificate2 certificate = CreateCertificate();
        Mock<IPlatformKeyVault> keyVault = new();
        keyVault.Setup(k => k.GetCertificateAsync("platform-access-token-private-cert")).ReturnsAsync(certificate);
        using MemoryCache cache = new(new MemoryCacheOptions());
        AccessTokenService sut = new(
            keyVault.Object,
            new AccessTokenGenerator(
                NullLogger<AccessTokenGenerator>.Instance,
                Options.Create(new AccessTokenSettings())
            ),
            cache
        );

        // Act
        string first = await sut.Generate();
        string second = await sut.Generate();

        // Assert
        JwtSecurityToken token = new JwtSecurityTokenHandler().ReadJwtToken(first);
        Assert.Equal("platform", token.Issuer);
        Assert.Equal("file-scan", token.Claims.Single(claim => claim.Type == "urn:altinn:app").Value);
        Assert.Equal(new X509SecurityKey(certificate).KeyId, token.Header.Kid);
        Assert.Equal(first, second);
        keyVault.Verify(k => k.GetCertificateAsync(It.IsAny<string>()), Times.Once);
    }

    /// <summary>
    /// Scenario: Key Vault has no usable certificate.
    /// Expected result: The missing certificate is not cached.
    /// Success criteria: The next call asks Key Vault again.
    /// </summary>
    [Fact]
    public async Task Generate_NoCertificate_AsksKeyVaultAgain()
    {
        // Arrange
        Mock<IPlatformKeyVault> keyVault = new();
        keyVault.Setup(k => k.GetCertificateAsync(It.IsAny<string>())).ReturnsAsync((X509Certificate2)null!);
        using MemoryCache cache = new(new MemoryCacheOptions());
        AccessTokenService sut = new(keyVault.Object, Mock.Of<IAccessTokenGenerator>(), cache);

        // Act
        await sut.Generate();
        await sut.Generate();

        // Assert
        keyVault.Verify(k => k.GetCertificateAsync(It.IsAny<string>()), Times.Exactly(2));
    }

    private static X509Certificate2 CreateCertificate()
    {
        using RSA rsa = RSA.Create(2048);
        CertificateRequest request = new("CN=platform-test", rsa, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
        return request.CreateSelfSigned(DateTimeOffset.UtcNow.AddMinutes(-5), DateTimeOffset.UtcNow.AddHours(1));
    }
}
