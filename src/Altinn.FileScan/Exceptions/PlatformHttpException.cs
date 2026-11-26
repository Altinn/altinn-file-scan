using System;
using System.Net;

namespace Altinn.FileScan.Exceptions;

/// <summary>
/// Exception class to hold exceptions when talking to the platform REST services
/// </summary>
/// <remarks>
/// Keeps only the status code, not the response: the request behind a response holds its URL and headers, which can
/// carry a credential, such as a read URL from Storage or a platform access token.
/// </remarks>
/// <param name="statusCode">The status code of the response.</param>
/// <param name="message">A description of the cause of the exception.</param>
public class PlatformHttpException(HttpStatusCode statusCode, string message) : Exception(message)
{
    /// <summary>
    /// Gets the status code of the response.
    /// </summary>
    public HttpStatusCode StatusCode { get; } = statusCode;
}
