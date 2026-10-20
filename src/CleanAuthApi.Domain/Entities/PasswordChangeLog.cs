using CleanAuthApi.Domain.Common;

namespace CleanAuthApi.Domain.Entities;

public class PasswordChangeLog : BaseEntity
{
    public Guid UserId { get; set; }
    public string Email { get; set; } = string.Empty;
    public bool IsSuccess { get; set; }
    public string? FailureReason { get; set; }
    public string? IpAddress { get; set; }
    public string? UserAgent { get; set; }
}
