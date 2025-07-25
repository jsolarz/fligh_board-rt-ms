using Microsoft.AspNetCore.Mvc;

namespace FlightBoard.Api.Attributes;

/// <summary>
/// Custom attribute for API versioning with caching support
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public class ApiVersionedAttribute : Attribute
{
    public string Version { get; }
    public int CacheDurationSeconds { get; set; } = 0;
    public bool IsDeprecated { get; set; } = false;
    public string? DeprecationMessage { get; set; }

    public ApiVersionedAttribute(string version)
    {
        Version = version;
    }
}

/// <summary>
/// Attribute for marking high-performance endpoints
/// </summary>
[AttributeUsage(AttributeTargets.Method)]
public class HighPerformanceAttribute : Attribute
{
    public bool EnableCompression { get; set; } = true;
    public bool EnableCaching { get; set; } = true;
    public int CacheDurationSeconds { get; set; } = 300;
    public string? CacheProfile { get; set; }
}

/// <summary>
/// Attribute for response compression configuration
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public class CompressionAttribute : Attribute
{
    public bool Enabled { get; set; } = true;
    public CompressionLevel Level { get; set; } = CompressionLevel.Optimal;
    public string[]? MimeTypes { get; set; }

    public enum CompressionLevel
    {
        Fastest,
        Optimal,
        NoCompression
    }
}
