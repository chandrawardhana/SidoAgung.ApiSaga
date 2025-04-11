using Microsoft.Extensions.Logging;
using System.Net;
using System.Text.Json;

namespace SidoAgung.ApiSaga.Infrastruktur.Middleware;

public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;
    private readonly IWebHostEnvironment _env;

    public ExceptionHandlingMiddleware(
        RequestDelegate next,
        ILogger<ExceptionHandlingMiddleware> logger,
        IWebHostEnvironment env)
    {
        _next = next;
        _logger = logger;
        _env = env;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        _logger.LogError(exception, "Terjadi error saat memproses request: {Message}", exception.Message);

        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;

        // Definisikan sebuah objek respons dengan struktur yang sama
        var response = new ErrorResponse
        {
            StatusCode = context.Response.StatusCode,
            Message = "Terjadi kesalahan internal server."
        };

        // Tambahkan detail tambahan jika dalam mode development
        if (_env.IsDevelopment())
        {
            response.DetailedMessage = exception.Message;
            response.StackTrace = exception.StackTrace;
        }

        var jsonResponse = JsonSerializer.Serialize(response, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        await context.Response.WriteAsync(jsonResponse);
    }
}

// Kelas untuk struktur respons error
public class ErrorResponse
{
    public int StatusCode { get; set; }
    public string Message { get; set; } = string.Empty;
    public string? DetailedMessage { get; set; }
    public string? StackTrace { get; set; }
}

// Extension method untuk menambahkan middleware ke pipeline
public static class ExceptionHandlingMiddlewareExtensions
{
    public static IApplicationBuilder UseGlobalExceptionHandler(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<ExceptionHandlingMiddleware>();
    }
}