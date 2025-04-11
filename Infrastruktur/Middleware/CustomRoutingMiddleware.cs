using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace SidoAgung.ApiSaga.Infrastruktur.Middleware;

public class CustomRoutingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<CustomRoutingMiddleware> _logger;

    public CustomRoutingMiddleware(RequestDelegate next, ILogger<CustomRoutingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var path = context.Request.Path.Value?.ToLower();

        // Custom redirect atau route handling
        if (path == "/api/lama" || path == "/apilama")
        {
            _logger.LogInformation("Mengarahkan ulang dari rute lama ke rute baru");
            context.Request.Path = "/wongnormal/customer";
            
            context.Request.Headers.Append("X-Route-Redirected", "true");
        }
        
        // Contoh URL khusus
        else if (path == "/status" || path == "/healthcheck")
        {
            _logger.LogInformation("Permintaan status API diterima");
            context.Response.ContentType = "application/json";
            await context.Response.WriteAsync("{\"status\": \"OK\", \"message\": \"Sistem berjalan dengan baik\"}");
            return; // Penting: Jangan lanjutkan ke middleware berikutnya
        }
        
        // Versioning API sederhana berbasis path
        else if (path?.StartsWith("/v1/") == true)
        {
            _logger.LogInformation("Mengarahkan API v1 ke endpoint default");
            context.Request.Path = path.Replace("/v1/", "/wongnormal/");
        }

        // Lanjutkan ke middleware berikutnya
        await _next(context);
    }
}

// Extension method untuk menambahkan middleware ke pipeline
public static class CustomRoutingMiddlewareExtensions
{
    public static IApplicationBuilder UseCustomRouting(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<CustomRoutingMiddleware>();
    }
}
