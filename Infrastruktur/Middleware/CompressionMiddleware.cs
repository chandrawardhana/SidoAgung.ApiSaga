using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.IO;
using System.IO.Compression;
using System.Threading.Tasks;

namespace SidoAgung.ApiSaga.Infrastruktur.Middleware;

public class CompressionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<CompressionMiddleware> _logger;

    // Jenis MIME yang dapat dikompresi
    private readonly string[] _compressibleTypes = new[]
    {
        "text/plain",
        "text/html",
        "text/css",
        "text/javascript",
        "application/javascript",
        "application/json",
        "application/xml",
        "application/x-font-ttf",
        "image/svg+xml"
    };

    public CompressionMiddleware(RequestDelegate next, ILogger<CompressionMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // Tangani kompresi untuk response
        var acceptEncoding = context.Request.Headers["Accept-Encoding"].ToString().ToLowerInvariant();
        
        // Jika client mendukung kompresi gzip atau brotli
        if (acceptEncoding.Contains("gzip") || acceptEncoding.Contains("br"))
        {
            // Simpan body asli dan ganti dengan stream yang dapat dikompresi
            var originalBodyStream = context.Response.Body;
            using var responseBodyStream = new MemoryStream();
            context.Response.Body = responseBodyStream;

            try
            {
                // Lanjutkan pipeline untuk memproses request
                await _next(context);

                // Periksa jika jenis konten response dapat dikompresi
                var contentType = context.Response.ContentType?.ToLowerInvariant() ?? "";
                bool shouldCompress = _compressibleTypes.Any(t => contentType.StartsWith(t)) &&
                                     responseBodyStream.Length > 1024; // Hanya kompresi untuk response > 1KB

                // Atur posisi stream ke awal
                responseBodyStream.Seek(0, SeekOrigin.Begin);

                // Terapkan kompresi jika sesuai
                if (shouldCompress)
                {
                    if (acceptEncoding.Contains("br"))
                    {
                        _logger.LogInformation($"Mengompres respons menggunakan Brotli: {context.Request.Path}");
                        context.Response.Headers.Append("Content-Encoding", "br");
                        await CompressBrotli(responseBodyStream, originalBodyStream);
                    }
                    else if (acceptEncoding.Contains("gzip"))
                    {
                        _logger.LogInformation($"Mengompres respons menggunakan GZip: {context.Request.Path}");
                        context.Response.Headers.Append("Content-Encoding", "gzip");
                        await CompressGZip(responseBodyStream, originalBodyStream);
                    }
                }
                else
                {
                    // Jika tidak dikompresi, salin langsung
                    await responseBodyStream.CopyToAsync(originalBodyStream);
                }
            }
            finally
            {
                // Kembalikan body asli
                context.Response.Body = originalBodyStream;
            }
        }
        else
        {
            // Jika client tidak mendukung kompresi, lanjutkan tanpa kompresi
            await _next(context);
        }
    }

    private async Task CompressGZip(Stream source, Stream destination)
    {
        using var gzipStream = new GZipStream(destination, CompressionLevel.Optimal, leaveOpen: true);
        await source.CopyToAsync(gzipStream);
    }

    private async Task CompressBrotli(Stream source, Stream destination)
    {
        using var brotliStream = new BrotliStream(destination, CompressionLevel.Optimal, leaveOpen: true);
        await source.CopyToAsync(brotliStream);
    }
}

// Extension method untuk menambahkan middleware ke pipeline
public static class CompressionMiddlewareExtensions
{
    public static IApplicationBuilder UseCustomCompression(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<CompressionMiddleware>();
    }
}
