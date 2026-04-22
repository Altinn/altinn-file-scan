using System.Net;

namespace Altinn.FileScan.Exceptions;

/// <summary>
/// An exception class related to non expected http response from the Muescheli Service
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="MuescheliHttpException"/> class.
/// </remarks>
public class MuescheliHttpException(HttpResponseMessage response, string message) : Exception(message)
{
    /// <summary>
    /// The http response message that generated the exception
    /// </summary>
    public HttpResponseMessage Response { get; } = response;

    /// <summary>
    /// Creates a new <see cref="MuescheliHttpException"/> combining the response message and
    /// </summary>
    public static async Task<MuescheliHttpException> CreateAsync(HttpStatusCode statusCode, HttpResponseMessage response)
    {
        string responseMessage = await response.Content.ReadAsStringAsync();

        string message = $"{statusCode} - {responseMessage}";
        return new MuescheliHttpException(response, message);
    }
}
