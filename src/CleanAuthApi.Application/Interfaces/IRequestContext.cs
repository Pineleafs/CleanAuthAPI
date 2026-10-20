namespace CleanAuthApi.Application.Interfaces;

public interface IRequestContext
{
    Guid? UserId { get; }
    string? IpAddress { get; }
    string? UserAgent { get; }
    string? RequestPath { get; }
}
