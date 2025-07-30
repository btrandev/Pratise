namespace Common.Middleware.Options;

/// <summary>
/// Options for configuring response caching middleware
/// </summary>
public class CachingOptions
{
    /// <summary>
    /// Gets or sets the response cache size limit in bytes. Default is 100MB.
    /// </summary>
    public long SizeLimitInBytes { get; set; } = 100 * 1024 * 1024;
    
    /// <summary>
    /// Gets or sets the maximum size of the cache entry. Default is 64KB.
    /// </summary>
    public long MaximumBodySizeInBytes { get; set; } = 64 * 1024;
    
    /// <summary>
    /// Gets or sets whether to use response caching service. Default is true.
    /// </summary>
    public bool UseCachingService { get; set; } = true;
    
    /// <summary>
    /// Gets or sets whether to use response caching middleware. Default is true.
    /// </summary>
    public bool UseCachingMiddleware { get; set; } = true;
    
    /// <summary>
    /// Gets or sets cache profile settings.
    /// </summary>
    public Dictionary<string, CacheProfile> CacheProfiles { get; set; } = new Dictionary<string, CacheProfile>
    {
        ["Default"] = new CacheProfile { Duration = 60, VaryByQueryKeys = new[] { "*" } },
        ["Static"] = new CacheProfile { Duration = 3600 },
        ["Api"] = new CacheProfile { Duration = 10, VaryByQueryKeys = new[] { "*" } }
    };
}

/// <summary>
/// Represents a cache profile for response caching
/// </summary>
public class CacheProfile
{
    /// <summary>
    /// Gets or sets the duration in seconds for which the response is cached.
    /// </summary>
    public int Duration { get; set; }
    
    /// <summary>
    /// Gets or sets the query keys to vary by.
    /// </summary>
    public string[] VaryByQueryKeys { get; set; } = Array.Empty<string>();
    
    /// <summary>
    /// Gets or sets the value for the 'public' parameter of the 'Cache-Control' header.
    /// </summary>
    public bool? IsPublic { get; set; } = true;
    
    /// <summary>
    /// Gets or sets the value for the 'must-revalidate' parameter of the 'Cache-Control' header.
    /// </summary>
    public bool MustRevalidate { get; set; }
    
    /// <summary>
    /// Gets or sets the value for the 'Location' parameter of the 'Cache-Control' header.
    /// </summary>
    public string Location { get; set; } = "Any";
    
    /// <summary>
    /// Gets or sets the value for the 'no-store' parameter of the 'Cache-Control' header.
    /// </summary>
    public bool NoStore { get; set; }
}
