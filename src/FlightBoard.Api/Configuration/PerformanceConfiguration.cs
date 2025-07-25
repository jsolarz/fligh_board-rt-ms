using Microsoft.AspNetCore.ResponseCompression;
using System.IO.Compression;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Asp.Versioning;

namespace FlightBoard.Api.Configuration;

/// <summary>
/// Extension methods for configuring performance optimizations
/// </summary>
public static class PerformanceConfiguration
{
    /// <summary>
    /// Add comprehensive performance optimizations including compression, caching, and JSON optimization
    /// </summary>
    public static IServiceCollection AddPerformanceOptimizations(this IServiceCollection services)
    {
        // 1. Add response compression
        services.AddResponseCompression(options =>
        {
            options.Providers.Add<BrotliCompressionProvider>();
            options.Providers.Add<GzipCompressionProvider>();
            options.MimeTypes = ResponseCompressionDefaults.MimeTypes.Concat(new[]
            {
                "application/json",
                "application/javascript",
                "text/json",
                "text/plain",
                "text/css",
                "application/xml",
                "text/xml",
                "application/atom+xml",
                "text/html"
            });
            options.EnableForHttps = true;
        });

        // Configure compression levels
        services.Configure<BrotliCompressionProviderOptions>(options =>
        {
            options.Level = CompressionLevel.Optimal;
        });

        services.Configure<GzipCompressionProviderOptions>(options =>
        {
            options.Level = CompressionLevel.Optimal;
        });

        // 2. Add response caching
        services.AddResponseCaching(options =>
        {
            options.MaximumBodySize = 1024 * 1024; // 1MB
            options.UseCaseSensitivePaths = false;
            options.SizeLimit = 50 * 1024 * 1024; // 50MB total cache size
        });

        // 3. Optimize JSON serialization
        services.ConfigureHttpJsonOptions(options =>
        {
            options.SerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
            options.SerializerOptions.WriteIndented = false;
            options.SerializerOptions.PropertyNameCaseInsensitive = true;
            options.SerializerOptions.DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull;
            options.SerializerOptions.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter());
            
            // Performance optimizations
            options.SerializerOptions.IgnoreReadOnlyProperties = false;
            options.SerializerOptions.IncludeFields = false;
            options.SerializerOptions.MaxDepth = 32;
            options.SerializerOptions.NumberHandling = System.Text.Json.Serialization.JsonNumberHandling.AllowReadingFromString;
        });

        // Configure JSON options for controllers
        services.Configure<JsonOptions>(options =>
        {
            options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
            options.JsonSerializerOptions.WriteIndented = false;
            options.JsonSerializerOptions.PropertyNameCaseInsensitive = true;
            options.JsonSerializerOptions.DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull;
            options.JsonSerializerOptions.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter());
            
            // Performance optimizations
            options.JsonSerializerOptions.IgnoreReadOnlyProperties = false;
            options.JsonSerializerOptions.IncludeFields = false;
            options.JsonSerializerOptions.MaxDepth = 32;
            options.JsonSerializerOptions.NumberHandling = System.Text.Json.Serialization.JsonNumberHandling.AllowReadingFromString;
        });

        // 4. Add API versioning
        services.AddApiVersioning(options =>
        {
            options.DefaultApiVersion = new ApiVersion(1, 0);
            options.AssumeDefaultVersionWhenUnspecified = true;
            options.ApiVersionReader = ApiVersionReader.Combine(
                new UrlSegmentApiVersionReader(),
                new QueryStringApiVersionReader("version"),
                new HeaderApiVersionReader("X-API-Version"),
                new MediaTypeApiVersionReader("version")
            );
            options.ApiVersionSelector = new CurrentImplementationApiVersionSelector(options);
        }).AddApiExplorer(options =>
        {
            // Format version as 'v1.0', 'v2.0', etc.
            options.GroupNameFormat = "'v'VVV";
            options.SubstituteApiVersionInUrl = true;
        });

        return services;
    }

    /// <summary>
    /// Configure performance middleware in the correct order
    /// </summary>
    public static WebApplication UsePerformanceOptimizations(this WebApplication app)
    {
        // Response compression should be early in the pipeline
        app.UseResponseCompression();
        
        // Response caching
        app.UseResponseCaching();
        
        return app;
    }
}
