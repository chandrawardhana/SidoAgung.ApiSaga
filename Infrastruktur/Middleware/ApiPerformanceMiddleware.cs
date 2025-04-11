using System.Diagnostics;
using Microsoft.Extensions.Logging;

namespace SidoAgung.ApiSaga.Infrastruktur.Middleware;

public class ApiPerformanceMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ApiPerformanceMiddleware> _logger;
    private readonly string _instanceId;

    public ApiPerformanceMiddleware(RequestDelegate next, ILogger<ApiPerformanceMiddleware> logger)
    {
        _next = next;
        _logger = logger;
        _instanceId = Guid.NewGuid().ToString().Substring(0, 8);
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var sw = Stopwatch.StartNew();
        var requestPath = context.Request.Path;
        var requestMethod = context.Request.Method;

        // Tambahkan identifikasi instance ke header response
        context.Response.Headers.Append("X-Instance-ID", _instanceId);

        try
        {
            await _next(context);
        }
        finally
        {
            sw.Stop();
            var elapsed = sw.ElapsedMilliseconds;

            // Catat performa
            var statusCode = context.Response.StatusCode;
            var performanceLog = $"{requestMethod} {requestPath} - Status: {statusCode}, Durasi: {elapsed}ms";

            // Catat performa berdasarkan kecepatan response
            if (elapsed < 500)
            {
                _logger.LogInformation(performanceLog);
            }
            else if (elapsed < 1000)
            {
                _logger.LogWarning($"Kinerja lambat: {performanceLog}");
            }
            else
            {
                _logger.LogError($"Kinerja sangat lambat: {performanceLog}");
            }

            // Tambahkan header X-Response-Time
            context.Response.Headers.Append("X-Response-Time", $"{elapsed}ms");
        }
    }
}

// Extension method untuk menambahkan middleware ke pipeline
public static class ApiPerformanceMiddlewareExtensions
{
    public static IApplicationBuilder UseApiPerformanceTracking(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<ApiPerformanceMiddleware>();
    }
}
