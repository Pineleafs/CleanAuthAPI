using System.Data.Common;
using CleanAuthApi.Application.DTOs.SecurityLogs;
using CleanAuthApi.Application.Interfaces;
using CleanAuthApi.Domain.Entities;
using CleanAuthApi.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace CleanAuthApi.Infrastructure.Services;

public class SecurityLogService(
    IServiceScopeFactory scopeFactory,
    IRequestContext requestContext,
    ILogger<SecurityLogService> logger) : ISecurityLogService
{
    public async Task LogDatabaseErrorAsync(
        Exception exception,
        string? sourceOperation = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            using var scope = scopeFactory.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            var baseEx = exception.GetBaseException();
            var errorCode = (baseEx as DbException)?.SqlState ?? baseEx.HResult.ToString();

            var log = new DatabaseErrorLog
            {
                ErrorType = baseEx.GetType().Name,
                ErrorCode = errorCode,
                Message = baseEx.Message,
                StackTrace = exception.StackTrace,
                SourceOperation = sourceOperation ?? requestContext.RequestPath,
                UserId = requestContext.UserId,
                IpAddress = requestContext.IpAddress
            };

            await dbContext.DatabaseErrorLogs.AddAsync(log, cancellationToken);
            await dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (Exception logEx)
        {
            logger.LogError(logEx, "Veritabanı hatası loglanırken ikincil bir hata oluştu.");
        }
    }

    public async Task LogFailedLoginAsync(
        string email,
        Guid? userId,
        string failureReason,
        CancellationToken cancellationToken = default)
    {
        try
        {
            using var scope = scopeFactory.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            var log = new FailedLoginLog
            {
                Email = email,
                UserId = userId,
                FailureReason = failureReason,
                IpAddress = requestContext.IpAddress,
                UserAgent = requestContext.UserAgent
            };

            await dbContext.FailedLoginLogs.AddAsync(log, cancellationToken);
            await dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Başarısız giriş denemesi loglanamadı.");
        }
    }

    public async Task LogPasswordChangeAsync(
        Guid userId,
        string email,
        bool isSuccess,
        string? failureReason = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            using var scope = scopeFactory.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            var log = new PasswordChangeLog
            {
                UserId = userId,
                Email = email,
                IsSuccess = isSuccess,
                FailureReason = failureReason,
                IpAddress = requestContext.IpAddress,
                UserAgent = requestContext.UserAgent
            };

            await dbContext.PasswordChangeLogs.AddAsync(log, cancellationToken);
            await dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Şifre değişikliği loglanamadı.");
        }
    }

    public async Task LogSecurityAuditAsync(
        Guid? userId,
        string? email,
        string eventType,
        string description,
        CancellationToken cancellationToken = default)
    {
        try
        {
            using var scope = scopeFactory.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            var log = new SecurityAuditLog
            {
                UserId = userId,
                Email = email,
                EventType = eventType,
                Description = description,
                IpAddress = requestContext.IpAddress,
                UserAgent = requestContext.UserAgent
            };

            await dbContext.SecurityAuditLogs.AddAsync(log, cancellationToken);
            await dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Güvenlik denetim kaydı (SecurityAuditLog) oluşturulamadı.");
        }
    }

    public async Task<IReadOnlyList<DatabaseErrorLogDto>> GetRecentDatabaseErrorsAsync(int count = 50, CancellationToken cancellationToken = default)
    {
        using var scope = scopeFactory.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        return await dbContext.DatabaseErrorLogs
            .AsNoTracking()
            .OrderByDescending(x => x.CreatedAt)
            .Take(count)
            .Select(x => new DatabaseErrorLogDto(
                x.Id, x.ErrorType, x.ErrorCode, x.Message, x.SourceOperation, x.UserId, x.IpAddress, x.CreatedAt))
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<FailedLoginLogDto>> GetRecentFailedLoginsAsync(int count = 50, CancellationToken cancellationToken = default)
    {
        using var scope = scopeFactory.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        return await dbContext.FailedLoginLogs
            .AsNoTracking()
            .OrderByDescending(x => x.CreatedAt)
            .Take(count)
            .Select(x => new FailedLoginLogDto(
                x.Id, x.Email, x.UserId, x.FailureReason, x.IpAddress, x.UserAgent, x.CreatedAt))
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<PasswordChangeLogDto>> GetRecentPasswordChangesAsync(int count = 50, CancellationToken cancellationToken = default)
    {
        using var scope = scopeFactory.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        return await dbContext.PasswordChangeLogs
            .AsNoTracking()
            .OrderByDescending(x => x.CreatedAt)
            .Take(count)
            .Select(x => new PasswordChangeLogDto(
                x.Id, x.UserId, x.Email, x.IsSuccess, x.FailureReason, x.IpAddress, x.UserAgent, x.CreatedAt))
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<SecurityAuditLogDto>> GetRecentSecurityAuditsAsync(int count = 50, CancellationToken cancellationToken = default)
    {
        using var scope = scopeFactory.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        return await dbContext.SecurityAuditLogs
            .AsNoTracking()
            .OrderByDescending(x => x.CreatedAt)
            .Take(count)
            .Select(x => new SecurityAuditLogDto(
                x.Id, x.UserId, x.Email, x.EventType, x.Description, x.IpAddress, x.UserAgent, x.CreatedAt))
            .ToListAsync(cancellationToken);
    }
}
