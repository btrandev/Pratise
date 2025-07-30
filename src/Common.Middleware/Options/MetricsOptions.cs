using System.Collections.Generic;

namespace Common.Middleware.Options;

/// <summary>
/// Options for configuring Prometheus metrics
/// </summary>
public class MetricsOptions
{
    /// <summary>
    /// Gets or sets the metrics endpoint path. Default is "/metrics".
    /// </summary>
    public string MetricsEndpoint { get; set; } = "/metrics";
    
    /// <summary>
    /// Gets or sets whether to include HTTP metrics. Default is true.
    /// </summary>
    public bool IncludeHttpMetrics { get; set; } = true;
    
    /// <summary>
    /// Gets or sets whether to include process metrics. Default is true.
    /// </summary>
    public bool IncludeProcessMetrics { get; set; } = true;
    
    /// <summary>
    /// Gets or sets custom labels to apply to all metrics. Default is empty.
    /// </summary>
    public Dictionary<string, string> CustomLabels { get; set; } = new Dictionary<string, string>();
    
    /// <summary>
    /// Gets or sets whether to suppress default metrics. Default is false.
    /// </summary>
    public bool SuppressDefaultMetrics { get; set; } = false;
}
