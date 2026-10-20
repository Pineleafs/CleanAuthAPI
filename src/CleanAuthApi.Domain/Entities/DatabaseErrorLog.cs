using CleanAuthApi.Domain.Common;

namespace CleanAuthApi.Domain.Entities;

public class DatabaseErrorLog : BaseEntity
{
    public string ErrorType { get; set; } = string.Empty;
    public string? ErrorCode { get; set; }
    public string Message { get; set; } = string.Empty;
    public string? StackTrace { get; set; }
    public string? SourceOperation { get; set; }
    public Guid? UserId { get; set; }
    public string? IpAddress { get; set; }
}
