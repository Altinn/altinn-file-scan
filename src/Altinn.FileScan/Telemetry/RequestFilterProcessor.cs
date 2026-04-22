#nullable disable

using System.Diagnostics;
using Microsoft.Extensions.Primitives;
using OpenTelemetry;

namespace Altinn.FileScan.Telemetry;

/// <summary>
/// Filter for requests (and child dependencies) that should not be logged.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="RequestFilterProcessor"/> class.
/// </remarks>
public class RequestFilterProcessor(IHttpContextAccessor httpContextAccessor = null) : BaseProcessor<Activity>()
{
    private const string RequestKind = "Microsoft.AspNetCore.Hosting.HttpRequestIn";

    /// <summary>
    /// Determine whether to skip a request
    /// </summary>
    public override void OnStart(Activity activity)
    {
        bool skip = false;
        if (activity.OperationName == RequestKind)
        {
            skip = ExcludeRequest(httpContextAccessor.HttpContext.Request.Path.Value);
        }
        else if (!(activity.Parent?.ActivityTraceFlags.HasFlag(ActivityTraceFlags.Recorded) ?? true))
        {
            skip = true;
        }

        if (skip)
        {
            activity.ActivityTraceFlags &= ~ActivityTraceFlags.Recorded;
        }
    }

    /// <summary>
    /// No action on end
    /// </summary>
    /// <param name="activity">xx</param>
    public override void OnEnd(Activity activity)
    {
        if (activity.OperationName == RequestKind && httpContextAccessor.HttpContext is not null &&
            httpContextAccessor.HttpContext.Request.Headers.TryGetValue("X-Forwarded-For", out StringValues ipAddress))
        {
            activity.SetTag("ipAddress", ipAddress.FirstOrDefault());
        }
    }

    private static bool ExcludeRequest(string localpath)
    {
        return localpath switch
        {
            var path when path.TrimEnd('/').EndsWith("/health", StringComparison.OrdinalIgnoreCase) => true,
            _ => false
        };
    }
}
