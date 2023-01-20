using System;
using System.Collections.Generic;
using System.Security.Claims;

using Altinn.Common.AccessToken.Constants;
using Altinn.FileScan.Tests.Mocks;

using AltinnCore.Authentication.Constants;

namespace Altinn.FileScan.Tests.Utils
{
    public static class PrincipalUtil
    {
        public static ClaimsPrincipal GetClaimsPrincipal(int userId, int authenticationLevel, string scope = null)
        {
            string issuer = "www.altinn.no";

            List<Claim> claims = new List<Claim>
            {
                new Claim(AltinnCoreClaimTypes.UserId, userId.ToString(), ClaimValueTypes.String, issuer),
                new Claim(AltinnCoreClaimTypes.UserName, "UserOne", ClaimValueTypes.String, issuer),
                new Claim(AltinnCoreClaimTypes.PartyID, userId.ToString(), ClaimValueTypes.Integer32, issuer),
                new Claim(AltinnCoreClaimTypes.AuthenticateMethod, "Mock", ClaimValueTypes.String, issuer),
                new Claim(AltinnCoreClaimTypes.AuthenticationLevel, authenticationLevel.ToString(), ClaimValueTypes.Integer32, issuer)
            };

            if (scope != null)
            {
                claims.Add(new Claim("urn:altinn:scope", scope, ClaimValueTypes.String, "maskinporten"));
            }

            ClaimsIdentity identity = new ClaimsIdentity("mock");
            identity.AddClaims(claims);

            return new ClaimsPrincipal(identity);
        }

        public static string GetToken(int userId, int authenticationLevel = 2, string scope = null)
        {
            ClaimsPrincipal principal = GetClaimsPrincipal(userId, authenticationLevel, scope);

            string token = JwtTokenMock.GenerateToken(principal, new TimeSpan(0, 1, 5));

            return token;
        }

        public static string GetAccessToken(string issuer, string app)
        {
            List<Claim> claims = new List<Claim>
            {
                new Claim(AccessTokenClaimTypes.App, app, ClaimValueTypes.String, issuer)
            };

            ClaimsIdentity identity = new ClaimsIdentity("mock");
            identity.AddClaims(claims);
            ClaimsPrincipal principal = new ClaimsPrincipal(identity);
            string token = JwtTokenMock.GenerateToken(principal, new TimeSpan(0, 1, 5), issuer);

            return token;
        }
    }
}
