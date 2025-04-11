using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.ResponseCaching;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace SidoAgung.ApiSaga.Infrastruktur.Middleware;

public class ResponseCachingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ResponseCachingMiddleware> _logger;

    public ResponseCachingMiddleware(RequestDelegate next, ILogger<ResponseCachingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // Hanya cache permintaan GET dan HEAD
        if (HttpMethods.IsGet(context.Request.Method) || HttpMethods.IsHead(context.Request.Method))
        {
            // Dapatkan variasi cache (misalnya berdasarkan query string)
            var cacheKey = context.Request.Path.ToString();
            if (context.Request.QueryString.HasValue)
            {
                cacheKey += context.Request.QueryString.Value;
            }

            _logger.LogInformation("Memproses permintaan cache untuk: {CacheKey}", cacheKey);

            // Atur header cache untuk respons
            context.Response.GetTypedHeaders().CacheControl = new Microsoft.Net.Http.Headers.CacheControlHeaderValue
            {
                Public = true,
                MaxAge = TimeSpan.FromSeconds(60) // Cache selama 60 detik
            };

            // Tambahkan header ETag untuk validasi cache
            var requestETag = context.Request.Headers.IfNoneMatch.ToString();
            var responseETag = $"\"{Guid.NewGuid():N}\"";
            context.Response.Headers.ETag = responseETag;

            // Jika ETag cocok, kembalikan 304 Not Modified
            if (!string.IsNullOrEmpty(requestETag) && requestETag == responseETag)
            {
                context.Response.StatusCode = StatusCodes.Status304NotModified;
                return;
            }
        }

        await _next(context);
    }
}

// Extension method untuk menambahkan middleware ke pipeline
public static class ResponseCachingMiddlewareExtensions
{
    public static IApplicationBuilder UseCustomResponseCaching(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<ResponseCachingMiddleware>();
    }
}
