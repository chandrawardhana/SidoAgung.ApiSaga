using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace SidoAgung.ApiSaga.Infrastruktur.Middleware;

public class RequestLoggingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<RequestLoggingMiddleware> _logger;

    public RequestLoggingMiddleware(RequestDelegate next, ILogger<RequestLoggingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var stopwatch = Stopwatch.StartNew();
        var requestTime = DateTime.UtcNow;
        var requestPath = context.Request.Path;
        var requestMethod = context.Request.Method;
        var requestId = Guid.NewGuid().ToString();

        // Catat informasi request
        _logger.LogInformation(
            "Request dimulai - ID: {RequestId}, Waktu: {RequestTime}, Method: {RequestMethod}, Path: {RequestPath}",
            requestId, requestTime, requestMethod, requestPath);

        // Tambahkan request ID ke response header
        context.Response.Headers.Append("X-Request-ID", requestId);

        try
        {
            await _next(context);
        }
        finally
        {
            stopwatch.Stop();
            var responseTime = stopwatch.ElapsedMilliseconds;
            var statusCode = context.Response.StatusCode;

            // Catat informasi response
            _logger.LogInformation(
                "Request selesai - ID: {RequestId}, StatusCode: {StatusCode}, Durasi: {Duration}ms",
                requestId, statusCode, responseTime);
        }
    }
}

// Extension method untuk menambahkan middleware ke pipeline
public static class RequestLoggingMiddlewareExtensions
{
    public static IApplicationBuilder UseRequestLogging(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<RequestLoggingMiddleware>();
    }
}
