using CleanAuthApi.Application.Common;
using CleanAuthApi.Application.DTOs.SecurityLogs;
using CleanAuthApi.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CleanAuthApi.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class SecurityLogsController(ISecurityLogService securityLogService) : ControllerBase
{
    [HttpGet("failed-logins")]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<FailedLoginLogDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetFailedLogins([FromQuery] int count = 50, CancellationToken cancellationToken = default)
    {
        var logs = await securityLogService.GetRecentFailedLoginsAsync(count, cancellationToken);
        return Ok(ApiResponse<IReadOnlyList<FailedLoginLogDto>>.Ok(logs));
    }

    [HttpGet("password-changes")]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<PasswordChangeLogDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetPasswordChanges([FromQuery] int count = 50, CancellationToken cancellationToken = default)
    {
        var logs = await securityLogService.GetRecentPasswordChangesAsync(count, cancellationToken);
        return Ok(ApiResponse<IReadOnlyList<PasswordChangeLogDto>>.Ok(logs));
    }

    [HttpGet("database-errors")]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<DatabaseErrorLogDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetDatabaseErrors([FromQuery] int count = 50, CancellationToken cancellationToken = default)
    {
        var logs = await securityLogService.GetRecentDatabaseErrorsAsync(count, cancellationToken);
        return Ok(ApiResponse<IReadOnlyList<DatabaseErrorLogDto>>.Ok(logs));
    }

    [HttpGet("audit-logs")]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<SecurityAuditLogDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAuditLogs([FromQuery] int count = 50, CancellationToken cancellationToken = default)
    {
        var logs = await securityLogService.GetRecentSecurityAuditsAsync(count, cancellationToken);
        return Ok(ApiResponse<IReadOnlyList<SecurityAuditLogDto>>.Ok(logs));
    }
}
