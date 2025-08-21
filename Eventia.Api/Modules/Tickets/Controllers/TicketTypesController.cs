using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Modules.Tickets.Domain;
using Modules.Auditing.Application;
using Shared.Application;
using System.Security.Claims;

namespace Modules.Tickets.Controllers;

[Authorize(Policy = "MANAGE_TICKETS")]
[ApiController]
[Route("api/ticket-types")]
public class TicketTypesController : ControllerBase
{
   private readonly ITicketTypeRepository _repo;
   private readonly IUnitOfWork _uow;
   private readonly IAuditService _auditService;

   public TicketTypesController(ITicketTypeRepository repo, IUnitOfWork uow, IAuditService auditService)
   {
       _repo = repo;
       _uow = uow;
       _auditService = auditService;
   }

   // Helper method para obtener el usuario actual
   private int GetCurrentUserId()
   {
       var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
       return int.TryParse(userIdClaim, out var userId) ? userId : 0;
   }

   [HttpGet]
   public async Task<IActionResult> GetAll() =>
       Ok(await _repo.ListAllAsync());

   [HttpPost]
   public async Task<IActionResult> Create([FromBody] CreateTicketTypeRequest req)
   {
       var currentUserId = GetCurrentUserId();
       var existing = await _repo.GetByNameAsync(req.Name.Trim());
       if (existing is not null) return Conflict("Ya existe un tipo con ese nombre");

       var type = new TicketType { Name = req.Name.Trim() };
       await _repo.AddAsync(type);
       await _uow.CommitAsync();

       // ✅ Log de auditoría
       await _auditService.LogGenericActionAsync(
           "TICKET_TYPE_CREATED",
           "TicketType",
           type.Id.ToString(),
           $"Tipo de ticket '{req.Name.Trim()}' creado",
           currentUserId,
           newValues: new { Name = req.Name.Trim() }
       );

       return CreatedAtAction(nameof(GetAll), new { id = type.Id }, type);
   }

   public record CreateTicketTypeRequest(string Name);
}