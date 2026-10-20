using System.Data.Common;
using System.Net;
using System.Text.Encodings.Web;
using System.Text.Json;
using CleanAuthApi.Application.Common;
using CleanAuthApi.Application.Common.Exceptions;
using CleanAuthApi.Application.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace CleanAuthApi.Api.Middleware;

public class GlobalExceptionMiddleware(
    RequestDelegate next,
    ILogger<GlobalExceptionMiddleware> logger,
    IHostEnvironment environment)
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
    };

    public async Task InvokeAsync(HttpContext context, ISecurityLogService securityLogService)
    {
        try
        {
            await next(context);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex, securityLogService);
        }
    }

    private async Task HandleExceptionAsync(
        HttpContext context,
        Exception exception,
        ISecurityLogService securityLogService)
    {
        var (statusCode, message) = exception switch
        {
            AppException appEx => (appEx.StatusCode, appEx.Message),
            UnauthorizedAccessException => ((int)HttpStatusCode.Unauthorized, "Bu işlem için yetkiniz bulunmamaktadır."),
            _ => ((int)HttpStatusCode.InternalServerError, "Sunucu tarafında beklenmeyen bir hata oluştu.")
        };

        if (statusCode == (int)HttpStatusCode.InternalServerError)
        {
            logger.LogError(exception, "Beklenmeyen bir hata oluştu: {Message}", exception.Message);

            // UnitOfWork.SaveChangesAsync dışında oluşan veritabanı / sistem hatalarını da logla
            if (exception is not (DbUpdateException or DbException))
            {
                await securityLogService.LogDatabaseErrorAsync(
                    exception,
                    $"{context.Request.Method} {context.Request.Path}");
            }
        }
        else
        {
            logger.LogWarning("İş kuralı hatası ({StatusCode}): {Message}", statusCode, message);
        }

        context.Response.ContentType = "application/json; charset=utf-8";
        context.Response.StatusCode = statusCode;

        IEnumerable<string>? errors = null;
        if (statusCode == (int)HttpStatusCode.InternalServerError && environment.IsDevelopment())
        {
            errors = [exception.Message, .. (exception.InnerException is not null ? new[] { exception.InnerException.Message } : [])];
        }

        var response = ApiResponse<object?>.Fail(message, errors);
        await context.Response.WriteAsync(JsonSerializer.Serialize(response, JsonOptions));
    }
}
