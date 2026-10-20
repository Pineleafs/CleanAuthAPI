using CleanAuthApi.Application.DTOs.SecurityLogs;

namespace CleanAuthApi.Application.Interfaces;

public interface ISecurityLogService
{
    Task LogDatabaseErrorAsync(
        Exception exception,
        string? sourceOperation = null,
        CancellationToken cancellationToken = default);

    Task LogFailedLoginAsync(
        string email,
        Guid? userId,
        string failureReason,
        CancellationToken cancellationToken = default);

    Task LogPasswordChangeAsync(
        Guid userId,
        string email,
        bool isSuccess,
        string? failureReason = null,
        CancellationToken cancellationToken = default);

    Task LogSecurityAuditAsync(
        Guid? userId,
        string? email,
        string eventType,
        string description,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<DatabaseErrorLogDto>> GetRecentDatabaseErrorsAsync(int count = 50, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<FailedLoginLogDto>> GetRecentFailedLoginsAsync(int count = 50, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<PasswordChangeLogDto>> GetRecentPasswordChangesAsync(int count = 50, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<SecurityAuditLogDto>> GetRecentSecurityAuditsAsync(int count = 50, CancellationToken cancellationToken = default);
}
