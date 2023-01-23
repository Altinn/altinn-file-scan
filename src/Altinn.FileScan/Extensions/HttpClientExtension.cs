namespace Altinn.FileScan.Extensions
{
    /// <summary>
    /// This extension is created to make it easy to add a bearer token to a HttpRequests.
    /// </summary>
    public static class HttpClientExtension
    {
        /// <summary>
        /// Extension that add authorization header to request
        /// </summary>
        /// <param name="httpClient">The HttpClient</param>
        /// <param name="requestUri">The request Uri</param>
        /// <param name="content">The http content</param>
        /// <param name="platformAccessToken">The platformAccess tokens</param>
        /// <returns>A HttpResponseMessage</returns>
        public static Task<HttpResponseMessage> PutAsync(this HttpClient httpClient, string requestUri, HttpContent content, string platformAccessToken)
        {
            HttpRequestMessage request = new(HttpMethod.Put, new Uri(requestUri, UriKind.Relative))
            {
                Content = content
            };

            if (!string.IsNullOrEmpty(platformAccessToken))
            {
                request.Headers.Add("PlatformAccessToken", platformAccessToken);
            }

            return httpClient.SendAsync(request, CancellationToken.None);
        }
    }
}
