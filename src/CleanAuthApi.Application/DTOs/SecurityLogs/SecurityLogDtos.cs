namespace CleanAuthApi.Application.DTOs.SecurityLogs;

public record DatabaseErrorLogDto(
    Guid Id,
    string ErrorType,
    string? ErrorCode,
    string Message,
    string? SourceOperation,
    Guid? UserId,
    string? IpAddress,
    DateTime CreatedAt
);

public record FailedLoginLogDto(
    Guid Id,
    string Email,
    Guid? UserId,
    string FailureReason,
    string? IpAddress,
    string? UserAgent,
    DateTime CreatedAt
);

public record PasswordChangeLogDto(
    Guid Id,
    Guid UserId,
    string Email,
    bool IsSuccess,
    string? FailureReason,
    string? IpAddress,
    string? UserAgent,
    DateTime CreatedAt
);

public record SecurityAuditLogDto(
    Guid Id,
    Guid? UserId,
    string? Email,
    string EventType,
    string Description,
    string? IpAddress,
    string? UserAgent,
    DateTime CreatedAt
);
