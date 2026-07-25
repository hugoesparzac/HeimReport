using HeimReport.Api.DTOs.AuditLogs;
using HeimReport.Api.DTOs.Common;
using HeimReport.Api.Services.AuditLogs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HeimReport.Api.Controllers;

[ApiController]
[Route("api/audit-logs")]
[Authorize(Roles = "Admin")]
public class AuditLogsController(IAuditLogService auditLogService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<PagedResultDto<AuditLogResponseDto>>> GetPaged(
        [FromQuery] AuditLogQueryDto query, CancellationToken cancellationToken)
    {
        var result = await auditLogService.GetPagedAsync(query, cancellationToken);
        return Ok(result);
    }
}