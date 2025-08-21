using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Modules.Tickets.Application;
using Modules.Tickets.Domain;
using Modules.Auditing.Application;
using System.Security.Claims;

namespace Modules.Tickets.Controllers;

[Authorize(Policy = "MANAGE_TICKETS")]
[ApiController]
[Route("api/tickets")]
public class TicketsController : ControllerBase
{
   private readonly ITicketAppService _service;
   private readonly IAuditService _auditService;

   public TicketsController(ITicketAppService service, IAuditService auditService)
   {
       _service = service;
       _auditService = auditService;
   }

   private int GetCurrentUserId()
   {
       var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
       return int.TryParse(userIdClaim, out var userId) ? userId : 0;
   }

   [HttpGet]
   public async Task<IActionResult> GetAll() => Ok(await _service.GetAllAsync());

   [HttpGet("{id:int}")]
   public async Task<IActionResult> GetById(int id)
   {
       var ticket = await _service.GetByIdAsync(id);
       return ticket is null ? NotFound() : Ok(ticket);
   }

   [HttpPost]
   public async Task<IActionResult> Create([FromBody] CreateTicketRequest req)
   {
       var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
       var currentUserId = GetCurrentUserId();
       var ticket = await _service.CreateAsync(req.Title, req.Description, req.EventId, req.AssignedUserId, req.TicketTypeId, userId);

       await _auditService.LogGenericActionAsync(
           "TICKET_CREATED",
           "Ticket",
           ticket.Id.ToString(),
           $"Ticket '{req.Title}' creado",
           currentUserId,
           newValues: new { 
               Title = req.Title, 
               Description = req.Description, 
               EventId = req.EventId, 
               AssignedUserId = req.AssignedUserId, 
               TicketTypeId = req.TicketTypeId 
           }
       );

       return CreatedAtAction(nameof(GetById), new { id = ticket.Id }, ticket);
   }

   [HttpPatch("{id:int}")]
   public async Task<IActionResult> Update(int id, [FromBody] UpdateTicketRequest req)
   {
       var currentUserId = GetCurrentUserId();
       var oldTicket = await _service.GetByIdAsync(id);
       if (oldTicket is null) return NotFound();

       var ticket = await _service.UpdateAsync(id, req.Title, req.Description, req.AssignedUserId);

       if (req.AssignedUserId.HasValue && req.AssignedUserId != oldTicket.AssignedUserId)
       {
           await _auditService.LogTicketAssignedAsync(id, req.AssignedUserId.Value, currentUserId);
       }

       if (req.Title != null || req.Description != null)
       {
           await _auditService.LogGenericActionAsync(
               "TICKET_UPDATED",
               "Ticket",
               id.ToString(),
               $"Ticket #{id} actualizado",
               currentUserId,
               oldValues: new { 
                   Title = oldTicket.Title, 
                   Description = oldTicket.Description 
               },
               newValues: new { 
                   Title = req.Title ?? oldTicket.Title, 
                   Description = req.Description ?? oldTicket.Description 
               }
           );
       }

       return Ok(ticket);
   }

   [HttpPatch("{id:int}/status")]
   public async Task<IActionResult> ChangeStatus(int id, [FromBody] ChangeStatusRequest req)
   {
       var currentUserId = GetCurrentUserId();
       var oldTicket = await _service.GetByIdAsync(id);
       if (oldTicket is null) return NotFound();

       var ticket = await _service.ChangeStatusAsync(id, req.Status);

       await _auditService.LogTicketStatusChangedAsync(id, oldTicket.Status.ToString(), req.Status.ToString(), currentUserId);

       return Ok(ticket);
   }

   [HttpDelete("{id:int}")]
   public async Task<IActionResult> Delete(int id)
   {
       var currentUserId = GetCurrentUserId();
       var ticket = await _service.GetByIdAsync(id);
       if (ticket is null) return NotFound();

       await _service.DeleteAsync(id);

       await _auditService.LogGenericActionAsync(
           "TICKET_DELETED",
           "Ticket",
           id.ToString(),
           $"Ticket '{ticket.Title}' eliminado",
           currentUserId,
           oldValues: new { 
               Title = ticket.Title, 
               Description = ticket.Description, 
               Status = ticket.Status.ToString() 
           }
       );

       return NoContent();
   }

   public record CreateTicketRequest(string Title, string Description, int EventId, int? AssignedUserId, int TicketTypeId);
   public record UpdateTicketRequest(string? Title, string? Description, int? AssignedUserId);
   public record ChangeStatusRequest(TicketStatus Status);
}