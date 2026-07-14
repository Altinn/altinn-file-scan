#nullable disable

namespace Altinn.FileScan.Configuration;

/// <summary>
/// Pooled connection options for the platform HTTP clients.
/// </summary>
public class HttpClientSettings
{
    /// <summary>
    /// Gets or sets how long an idle pooled connection is kept, in seconds.
    /// </summary>
    public int PooledConnectionIdleTimeoutSeconds { get; set; } = 4;

    /// <summary>
    /// Gets or sets the maximum lifetime of a pooled connection, in seconds.
    /// </summary>
    public int PooledConnectionLifetimeSeconds { get; set; } = 120;

    /// <summary>
    /// Gets or sets the connect timeout, in seconds.
    /// </summary>
    public int ConnectTimeoutSeconds { get; set; } = 5;
}
