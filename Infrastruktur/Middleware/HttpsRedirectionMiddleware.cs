using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace SidoAgung.ApiSaga.Infrastruktur.Middleware;

public class HttpsRedirectionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<HttpsRedirectionMiddleware> _logger;
    private readonly IWebHostEnvironment _env;

    public HttpsRedirectionMiddleware(
        RequestDelegate next,
        ILogger<HttpsRedirectionMiddleware> logger,
        IWebHostEnvironment env)
    {
        _next = next;
        _logger = logger;
        _env = env;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // Skip HTTPS redirection untuk localhost pada environment development
        bool isLocalhost = context.Request.Host.Host.Equals("localhost") ||
                          context.Request.Host.Host.Equals("127.0.0.1");

        // Hanya redirect jika bukan localhost atau environment production
        if (!context.Request.IsHttps && (!isLocalhost || !_env.IsDevelopment()))
        {
            string httpsUrl = $"https://{context.Request.Host}{context.Request.Path}{context.Request.QueryString}";
            _logger.LogInformation($"Mengalihkan permintaan ke HTTPS: {httpsUrl}");

            context.Response.StatusCode = StatusCodes.Status307TemporaryRedirect;
            context.Response.Headers.Location = httpsUrl;

            // Tambahkan header keamanan tambahan
            context.Response.Headers.Append("X-Redirected-From", "HTTP");
            context.Response.Headers.Append("X-Content-Type-Options", "nosniff");

            await context.Response.WriteAsync("Mengalihkan ke koneksi aman (HTTPS)...");
            return;
        }

        // Lanjutkan ke middleware berikutnya jika sudah HTTPS atau merupakan localhost di development
        await _next(context);
    }
}

// Extension method untuk menambahkan middleware ke pipeline
public static class HttpsRedirectionMiddlewareExtensions
{
    public static IApplicationBuilder UseCustomHttpsRedirection(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<HttpsRedirectionMiddleware>();
    }
}