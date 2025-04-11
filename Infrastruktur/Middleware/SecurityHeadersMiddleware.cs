namespace SidoAgung.ApiSaga.Infrastruktur.Middleware;

public class SecurityHeadersMiddleware
{
    private readonly RequestDelegate _next;

    public SecurityHeadersMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // Tambahkan header keamanan ke response
        context.Response.Headers.Append("X-Content-Type-Options", "nosniff");
        context.Response.Headers.Append("X-Frame-Options", "DENY");
        context.Response.Headers.Append("X-XSS-Protection", "1; mode=block");
        context.Response.Headers.Append("Referrer-Policy", "strict-origin-when-cross-origin");
        context.Response.Headers.Append("Permissions-Policy", "accelerometer=(), camera=(), geolocation=(), gyroscope=(), magnetometer=(), microphone=(), payment=(), usb=()");

        // Jika dalam mode development, jangan tambahkan HSTS dan CSP
        if (!context.Request.Host.Host.Contains("localhost") && 
            !context.Request.Host.Host.Equals("127.0.0.1"))
        {
            // Strict-Transport-Security
            context.Response.Headers.Append("Strict-Transport-Security", "max-age=31536000; includeSubDomains");
            
            // Content-Security-Policy
            context.Response.Headers.Append(
                "Content-Security-Policy",
                "default-src 'self'; " +
                "img-src 'self' data:; " +
                "font-src 'self'; " +
                "style-src 'self' 'unsafe-inline'; " +
                "script-src 'self' 'unsafe-inline' 'unsafe-eval'; " +
                "connect-src 'self'"
            );
        }

        await _next(context);
    }
}

// Extension method untuk menambahkan middleware ke pipeline
public static class SecurityHeadersMiddlewareExtensions
{
    public static IApplicationBuilder UseSecurityHeaders(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<SecurityHeadersMiddleware>();
    }
}
