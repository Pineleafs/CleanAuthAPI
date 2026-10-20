using System.Security.Claims;
using CleanAuthApi.Application.Interfaces;

namespace CleanAuthApi.Api.Services;

public class RequestContext(IHttpContextAccessor httpContextAccessor) : IRequestContext
{
    public Guid? UserId
    {
        get
        {
            var claim = httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier);
            return Guid.TryParse(claim, out var id) ? id : null;
        }
    }

    public string? IpAddress =>
        httpContextAccessor.HttpContext?.Connection?.RemoteIpAddress?.ToString();

    public string? UserAgent =>
        httpContextAccessor.HttpContext?.Request?.Headers.UserAgent.ToString();

    public string? RequestPath
    {
        get
        {
            var req = httpContextAccessor.HttpContext?.Request;
            return req is null ? null : $"{req.Method} {req.Path}";
        }
    }
}
