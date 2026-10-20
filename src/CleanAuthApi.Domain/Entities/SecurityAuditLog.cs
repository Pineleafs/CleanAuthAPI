using CleanAuthApi.Domain.Common;

namespace CleanAuthApi.Domain.Entities;

public class SecurityAuditLog : BaseEntity
{
    public Guid? UserId { get; set; }
    public string? Email { get; set; }
    public string EventType { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string? IpAddress { get; set; }
    public string? UserAgent { get; set; }
}
