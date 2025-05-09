using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography.X509Certificates;

using Microsoft.IdentityModel.Tokens;

namespace Altinn.FileScan.Tests.Mocks
{
    /// <summary>
    /// Represents a mechanism for creating JSON Web tokens for use in integration tests.
    /// </summary>
    public static class JwtTokenMock
    {
        /// <summary>
        /// Generates a token with a self signed certificate included in the integration test project.
        /// </summary>
        /// <returns>A new token.</returns>
        public static string GenerateToken(ClaimsPrincipal principal, TimeSpan tokenExipry, string issuer = "UnitTest")
        {
            JwtSecurityTokenHandler tokenHandler = new();
            SecurityTokenDescriptor tokenDescriptor = new()
            {
                Subject = new ClaimsIdentity(principal.Identity),
                Expires = DateTime.UtcNow.AddSeconds(tokenExipry.TotalSeconds),
                SigningCredentials = GetSigningCredentials(issuer),
                Audience = "altinn.no",
                Issuer = issuer
            };

            SecurityToken token = tokenHandler.CreateToken(tokenDescriptor);
            string tokenstring = tokenHandler.WriteToken(token);

            return tokenstring;
        }

        private static SigningCredentials GetSigningCredentials(string issuer)
        {
            string certPath = "jwtselfsignedcert.pfx";
            if (!issuer.Equals("UnitTest"))
            {
                certPath = $"{issuer}-org.pfx";

                X509Certificate2 certIssuer = X509CertificateLoader.LoadPkcs12FromFile(certPath, (string)null);

                return new X509SigningCredentials(certIssuer, SecurityAlgorithms.RsaSha256);
            }

            X509Certificate2 cert = X509CertificateLoader.LoadPkcs12FromFile(certPath, "qwer1234");

            return new X509SigningCredentials(cert, SecurityAlgorithms.RsaSha256);
        }
    }
}
