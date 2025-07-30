namespace Common.Middleware.Options;

/// <summary>
/// Options for configuring health checks
/// </summary>
public class HealthCheckOptions
{
    /// <summary>
    /// Gets or sets the health check endpoint path. Default is "/health".
    /// </summary>
    public string HealthCheckPath { get; set; } = "/health";
    
    /// <summary>
    /// Gets or sets the readiness probe path for Kubernetes. Default is "/health/ready".
    /// </summary>
    public string ReadinessPath { get; set; } = "/health/ready";
    
    /// <summary>
    /// Gets or sets the liveness probe path for Kubernetes. Default is "/health/live".
    /// </summary>
    public string LivenessPath { get; set; } = "/health/live";
    
    /// <summary>
    /// Gets or sets whether to use Kubernetes-style health checks. Default is false.
    /// </summary>
    public bool UseKubernetesProbes { get; set; } = false;
}
