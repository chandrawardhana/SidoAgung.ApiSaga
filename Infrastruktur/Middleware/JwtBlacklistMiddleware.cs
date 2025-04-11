using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace SidoAgung.ApiSaga.Infrastruktur.Middleware;

public class JwtBlacklistMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<JwtBlacklistMiddleware> _logger;
    private readonly AuthService _authService;

    public JwtBlacklistMiddleware(
        RequestDelegate next,
        ILogger<JwtBlacklistMiddleware> logger,
        AuthService authService)
    {
        _next = next;
        _logger = logger;
        _authService = authService;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // Periksa apakah request memiliki header Authorization
        if (context.Request.Headers.TryGetValue("Authorization", out var authHeader))
        {
            var token = authHeader.ToString().Replace("Bearer ", "");

            // Periksa apakah token ada dalam daftar token yang dibatalkan
            if (!string.IsNullOrEmpty(token) && _authService.IsTokenRevoked(token))
            {
                _logger.LogWarning("Akses ditolak: Token telah dibatalkan.");
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                await context.Response.WriteAsync("Token telah dibatalkan. Silakan login kembali.");
                return;
            }
        }

        await _next(context);
    }
}

// Extension method untuk menambahkan middleware ke pipeline
public static class JwtBlacklistMiddlewareExtensions
{
    public static IApplicationBuilder UseJwtBlacklist(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<JwtBlacklistMiddleware>();
    }
}
