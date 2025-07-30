using Microsoft.AspNetCore.ResponseCompression;
using System.IO.Compression;

namespace Common.Middleware.Options;

/// <summary>
/// Options for configuring response compression middleware
/// </summary>
public class CompressionOptions
{
    /// <summary>
    /// Gets or sets whether compression should be enabled for HTTPS requests
    /// </summary>
    public bool EnableForHttps { get; set; } = true;
    
    /// <summary>
    /// Gets or sets whether to use Gzip compression
    /// </summary>
    public bool UseGzip { get; set; } = true;
    
    /// <summary>
    /// Gets or sets whether to use Brotli compression
    /// </summary>
    public bool UseBrotli { get; set; } = true;
    
    /// <summary>
    /// Gets or sets the compression level
    /// </summary>
    public CompressionLevel CompressionLevel { get; set; } = CompressionLevel.Fastest;
    
    /// <summary>
    /// Gets or sets the MIME types to compress
    /// </summary>
    public IEnumerable<string> MimeTypes { get; set; } = ResponseCompressionDefaults.MimeTypes.Concat(
        new[] { "application/json", "application/xml", "text/css", "text/javascript" });
}
