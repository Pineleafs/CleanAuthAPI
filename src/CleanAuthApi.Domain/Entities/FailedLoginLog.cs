using CleanAuthApi.Domain.Common;

namespace CleanAuthApi.Domain.Entities;

public class FailedLoginLog : BaseEntity
{
    public string Email { get; set; } = string.Empty;
    public Guid? UserId { get; set; }
    public string FailureReason { get; set; } = string.Empty;
    public string? IpAddress { get; set; }
    public string? UserAgent { get; set; }
}
