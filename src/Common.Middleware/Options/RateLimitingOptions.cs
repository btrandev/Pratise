namespace Common.Middleware.Options;

/// <summary>
/// Options for configuring rate limiting middleware
/// </summary>
public class RateLimitingOptions
{
    /// <summary>
    /// Gets or sets the fixed window permit limit. Default is 100 requests.
    /// </summary>
    public int FixedWindowPermitLimit { get; set; } = 100;
    
    /// <summary>
    /// Gets or sets the fixed window duration in seconds. Default is 60 seconds (1 minute).
    /// </summary>
    public int FixedWindowDurationSeconds { get; set; } = 60;
    
    /// <summary>
    /// Gets or sets the fixed window queue limit. Default is 10 requests.
    /// </summary>
    public int FixedWindowQueueLimit { get; set; } = 10;
    
    /// <summary>
    /// Gets or sets the sliding window permit limit. Default is 100 requests.
    /// </summary>
    public int SlidingWindowPermitLimit { get; set; } = 100;
    
    /// <summary>
    /// Gets or sets the sliding window duration in seconds. Default is 60 seconds (1 minute).
    /// </summary>
    public int SlidingWindowDurationSeconds { get; set; } = 60;
    
    /// <summary>
    /// Gets or sets the sliding window segments. Default is 4 segments.
    /// </summary>
    public int SlidingWindowSegments { get; set; } = 4;
    
    /// <summary>
    /// Gets or sets the sliding window queue limit. Default is 10 requests.
    /// </summary>
    public int SlidingWindowQueueLimit { get; set; } = 10;
    
    /// <summary>
    /// Gets or sets the token bucket token limit. Default is 100 tokens.
    /// </summary>
    public int TokenBucketLimit { get; set; } = 100;
    
    /// <summary>
    /// Gets or sets the token bucket queue limit. Default is 10 requests.
    /// </summary>
    public int TokenBucketQueueLimit { get; set; } = 10;
    
    /// <summary>
    /// Gets or sets the token bucket replenishment period in seconds. Default is 10 seconds.
    /// </summary>
    public int TokenBucketReplenishmentPeriodSeconds { get; set; } = 10;
    
    /// <summary>
    /// Gets or sets the token bucket tokens per period. Default is 20 tokens.
    /// </summary>
    public int TokenBucketTokensPerPeriod { get; set; } = 20;
    
    /// <summary>
    /// Gets or sets the global permit limit. Default is 1000 requests.
    /// </summary>
    public int GlobalPermitLimit { get; set; } = 1000;
    
    /// <summary>
    /// Gets or sets the global window duration in seconds. Default is 60 seconds (1 minute).
    /// </summary>
    public int GlobalWindowDurationSeconds { get; set; } = 60;
    
    /// <summary>
    /// Gets or sets the global queue limit. Default is 100 requests.
    /// </summary>
    public int GlobalQueueLimit { get; set; } = 100;
    
    /// <summary>
    /// Gets or sets the custom rejection message. Default is "Too many requests. Please try again later."
    /// </summary>
    public string RejectionMessage { get; set; } = "Too many requests. Please try again later.";
    
    /// <summary>
    /// Gets or sets whether to enable the fixed window limiter. Default is true.
    /// </summary>
    public bool EnableFixedWindowLimiter { get; set; } = true;
    
    /// <summary>
    /// Gets or sets whether to enable the sliding window limiter. Default is true.
    /// </summary>
    public bool EnableSlidingWindowLimiter { get; set; } = true;
    
    /// <summary>
    /// Gets or sets whether to enable the token bucket limiter. Default is true.
    /// </summary>
    public bool EnableTokenBucketLimiter { get; set; } = true;
    
    /// <summary>
    /// Gets or sets whether to enable the global limiter. Default is true.
    /// </summary>
    public bool EnableGlobalLimiter { get; set; } = true;
}
