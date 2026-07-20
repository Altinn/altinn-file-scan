namespace Altinn.FileScan.Configuration;

/// <summary>
/// Pooled connection options for the platform HTTP clients.
/// </summary>
public class StorageClientSettings
{
    /// <summary>
    /// Gets or sets how long an idle pooled connection is kept, in seconds.
    /// </summary>
    public int PooledConnectionIdleTimeoutSeconds { get; set; }

    /// <summary>
    /// Gets or sets the maximum lifetime of a pooled connection, in seconds.
    /// </summary>
    public int PooledConnectionLifetimeSeconds { get; set; }

    /// <summary>
    /// Gets or sets the connect timeout, in seconds.
    /// </summary>
    public int ConnectTimeoutSeconds { get; set; }
}
