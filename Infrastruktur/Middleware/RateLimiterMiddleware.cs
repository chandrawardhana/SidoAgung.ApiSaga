using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Net;
using System.Threading.Tasks;

namespace SidoAgung.ApiSaga.Infrastruktur.Middleware;

public class RateLimiterOptions
{
    public int RequestLimit { get; set; } = 100;
    public int WindowMinutes { get; set; } = 15;
    public bool IncludeClientIpInKey { get; set; } = true;
}

public class RateLimiterMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<RateLimiterMiddleware> _logger;
    private readonly IMemoryCache _cache;
    private readonly RateLimiterOptions _options;

    public RateLimiterMiddleware(
        RequestDelegate next,
        ILogger<RateLimiterMiddleware> logger,
        IMemoryCache cache,
        IOptions<RateLimiterOptions> options)
    {
        _next = next;
        _logger = logger;
        _cache = cache;
        _options = options.Value;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // Buat kunci unik berdasarkan endpoint dan IP client (opsional)
        var endpoint = context.Request.Path.ToString();
        var clientIp = _options.IncludeClientIpInKey 
            ? context.Connection.RemoteIpAddress?.ToString() ?? "unknown"
            : "";
        
        var requestKey = $"rate_limit_{endpoint}_{clientIp}";
        
        // Cek apakah jumlah request sudah melebihi batas
        if (!_cache.TryGetValue(requestKey, out RateLimitCounter counter))
        {
            counter = new RateLimitCounter
            {
                Count = 0,
                FirstRequest = DateTime.UtcNow
            };
        }

        // Periksa jika window waktu sudah berakhir, reset counter
        var timeSinceFirstRequest = DateTime.UtcNow - counter.FirstRequest;
        if (timeSinceFirstRequest.TotalMinutes > _options.WindowMinutes)
        {
            counter = new RateLimitCounter
            {
                Count = 0,
                FirstRequest = DateTime.UtcNow
            };
        }

        // Tambah counter dan simpan kembali ke cache
        counter.Count++;
        
        var cacheEntryOptions = new MemoryCacheEntryOptions()
            .SetAbsoluteExpiration(TimeSpan.FromMinutes(_options.WindowMinutes));
        
        _cache.Set(requestKey, counter, cacheEntryOptions);

        // Kirim header rate limit ke client
        context.Response.Headers.Append("X-RateLimit-Limit", _options.RequestLimit.ToString());
        context.Response.Headers.Append("X-RateLimit-Remaining", Math.Max(0, _options.RequestLimit - counter.Count).ToString());
        context.Response.Headers.Append("X-RateLimit-Reset", counter.FirstRequest.AddMinutes(_options.WindowMinutes).ToString("o"));

        // Jika counter melebihi limit, kirim response 429 Too Many Requests
        if (counter.Count > _options.RequestLimit)
        {
            _logger.LogWarning("Rate limit terlampaui: {ClientIp} untuk {Endpoint}", clientIp, endpoint);
            
            context.Response.StatusCode = (int)HttpStatusCode.TooManyRequests;
            context.Response.Headers.Append("Retry-After", _options.WindowMinutes.ToString());
            
            await context.Response.WriteAsync($"Terlalu banyak permintaan. Coba lagi setelah {_options.WindowMinutes} menit.");
            return;
        }

        // Lanjutkan ke middleware berikutnya jika belum mencapai limit
        await _next(context);
    }
}


public class RateLimitCounter
{
    public int Count { get; set; }
    public DateTime FirstRequest { get; set; }
}


public static class RateLimiterMiddlewareExtensions
{
    public static IApplicationBuilder UseRateLimiter(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<RateLimiterMiddleware>();
    }

    public static IServiceCollection AddRateLimiter(this IServiceCollection services, Action<RateLimiterOptions> configureOptions = null)
    {
        if (configureOptions != null)
        {
            services.Configure(configureOptions);
        }
        else
        {
            services.Configure<RateLimiterOptions>(options => { });
        }

        // Pastikan IMemoryCache tersedia
        services.AddMemoryCache();
        
        return services;
    }
}
