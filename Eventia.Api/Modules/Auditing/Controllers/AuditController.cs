using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Modules.Auditing.Domain;

namespace Modules.Auditing.Controllers;

[Authorize(Policy = "MANAGE_AUDIT")]
[ApiController]
[Route("api/audit")]
public class AuditController : ControllerBase
{
    private readonly IAuditRepository _auditRepository;

    public AuditController(IAuditRepository auditRepository)
    {
        _auditRepository = auditRepository;
    }

    [HttpGet("recent")]
    public async Task<IActionResult> GetRecent([FromQuery] int count = 50)
    {
        var logs = await _auditRepository.GetRecentAsync(count);
        return Ok(logs);
    }

    [HttpGet("entity/{entityType}/{entityId}")]
    public async Task<IActionResult> GetByEntity(string entityType, string entityId)
    {
        var logs = await _auditRepository.GetByEntityAsync(entityType, entityId);
        return Ok(logs);
    }

    [HttpGet("user/{userId}")]
    public async Task<IActionResult> GetByUser(int userId)
    {
        var logs = await _auditRepository.GetByUserAsync(userId);
        return Ok(logs);
    }
}