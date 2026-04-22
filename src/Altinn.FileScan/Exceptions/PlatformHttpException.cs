using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace Altinn.FileScan.Exceptions;

/// <summary>
/// Exception class to hold exceptions when talking to the platform REST services
/// </summary>
/// <remarks>
/// Copy the response for further investigations
/// </remarks>
/// <param name="response">the response</param>
/// <param name="message">A description of the cause of the exception.</param>
public class PlatformHttpException(HttpResponseMessage response, string message) : Exception(message)
{
    /// <summary>
    /// Responsible for holding an http request exception towards platform (storage).
    /// </summary>
    public HttpResponseMessage Response { get; } = response;

    /// <summary>
    /// Create a new <see cref="PlatformHttpException"/> by reading the <see cref="HttpResponseMessage"/>
    /// content asynchronously.
    /// </summary>
    /// <param name="response">The <see cref="HttpResponseMessage"/> to read.</param>
    /// <returns>A new <see cref="PlatformHttpException"/>.</returns>
    public static async Task<PlatformHttpException> CreateAsync(HttpResponseMessage response)
    {
        string content = await response.Content.ReadAsStringAsync();
        string message = $"{(int)response.StatusCode} - {response.ReasonPhrase} - {content}";

        return new PlatformHttpException(response, message);
    }
}
