using System;
using System.Net;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace TESTPROJESI.Middlewares
{
    /// <summary>
    /// ⚙️ Tüm hataları global olarak yakalayan middleware.
    /// Hem kullanıcıya sade bir hata mesajı döner, hem de log’ları detaylı kaydeder.
    /// </summary>
    public class ErrorHandlingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ErrorHandlingMiddleware> _logger;

        public ErrorHandlingMiddleware(RequestDelegate next, ILogger<ErrorHandlingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context); // isteği devam ettir
            }
            catch (Exception ex)
            {
                await HandleExceptionAsync(context, ex);
            }
        }

        /// <summary>
        /// Hataları yakalar, loglar ve uygun HTTP yanıtı döner.
        /// </summary>
        private async Task HandleExceptionAsync(HttpContext context, Exception ex)
        {
            // 🧩 Request ID (RequestIdMiddleware tarafından atanır)
            var requestId = context.Items["RequestId"]?.ToString() ?? Guid.NewGuid().ToString("N").Substring(0, 8);

            // 🔍 Hata tipi
            var statusCode = ex switch
            {
                ArgumentNullException => HttpStatusCode.BadRequest,
                UnauthorizedAccessException => HttpStatusCode.Unauthorized,
                KeyNotFoundException => HttpStatusCode.NotFound,
                HttpRequestException => HttpStatusCode.BadGateway,
                TimeoutException => HttpStatusCode.RequestTimeout,
                _ => HttpStatusCode.InternalServerError
            };

            // 💾 Loglama (ayrıntılı + RequestId ile)
            _logger.LogError(ex,
                "💥 Global hata yakalandı! | RequestId: {RequestId} | Path: {Path} | Status: {StatusCode}",
                requestId, context.Request.Path, (int)statusCode);

            // 🧱 Log formatı (JSON response)
            var errorResponse = new
            {
                success = false,
                statusCode = (int)statusCode,
                requestId = requestId,
                message = ex.Message,
                detail = ex.InnerException?.Message,
                path = context.Request.Path,
                timestamp = DateTime.UtcNow.ToString("o")
            };

            // 💬 JSON yanıtı döndür
            context.Response.ContentType = "application/json";
            context.Response.StatusCode = (int)statusCode;

            var options = new JsonSerializerOptions { WriteIndented = true };
            var json = JsonSerializer.Serialize(errorResponse, options);
            await context.Response.WriteAsync(json);
        }
    }
}
