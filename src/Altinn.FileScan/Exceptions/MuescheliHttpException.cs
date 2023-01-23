using System.Net;

namespace Altinn.FileScan.Exceptions
{
    /// <summary>
    /// An exception class related to non expected http response from the Muescheli Service
    /// </summary>
    public class MuescheliHttpException : Exception
    {
        /// <summary>
        /// The http response message that generated the exception
        /// </summary>
        public HttpResponseMessage Response { get; }

        /// <summary>
        /// Creates a new <see cref="MuescheliHttpException"/> combining the response message and 
        /// </summary>
        public static async Task<MuescheliHttpException> CreateAsync(HttpStatusCode statusCode, HttpResponseMessage response)
        {
            string responseMessage = await response.Content.ReadAsStringAsync();

            string message = $"{statusCode} - {responseMessage}";
            return new MuescheliHttpException(response, message);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MuescheliHttpException"/> class.
        /// </summary>
        public MuescheliHttpException(HttpResponseMessage response, string message) : base(message)
        {
            Response = response;
        }
    }
}
