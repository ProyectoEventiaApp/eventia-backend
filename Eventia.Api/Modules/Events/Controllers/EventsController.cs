// EventsController.cs - Actualizado con nuevos campos
using Microsoft.AspNetCore.Mvc;
using Modules.Events.Domain;
using Modules.Auditing.Application;
using Shared.Application;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace Modules.Events.Controllers;

[Authorize(Policy = "MANAGE_EVENTS")]
[ApiController]
[Route("api/events")]
public class EventsController : ControllerBase
{
   private readonly IEventRepository _events;
   private readonly IUnitOfWork _uow;
   private readonly IAuditService _auditService;

   public EventsController(IEventRepository events, IUnitOfWork uow, IAuditService auditService)
   {
       _events = events;
       _uow = uow;
       _auditService = auditService;
   }

   private int GetCurrentUserId()
   {
       var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
       return int.TryParse(userIdClaim, out var userId) ? userId : 0;
   }

   public record CreateEventRequest(
       string Title, 
       string Description,
       DateTime StartDate,
       DateTime EndDate,
       string Location,
       int MaxAttendees
   );

   public record UpdateEventRequest(
       string Title, 
       string Description,
       DateTime StartDate,
       DateTime EndDate,
       string Location,
       int MaxAttendees
   );

   [HttpPost]
   public async Task<IActionResult> Create([FromBody] CreateEventRequest req)
   {
       var currentUserId = GetCurrentUserId();
       
       // Validaciones básicas
       if (req.StartDate >= req.EndDate)
           return BadRequest("La fecha de inicio debe ser anterior a la fecha de fin");
           
       if (req.MaxAttendees < 0)
           return BadRequest("El máximo de asistentes no puede ser negativo");
       
       var ev = new Event
       {
           Title = req.Title,
           Description = req.Description,
           StartDate = req.StartDate,
           EndDate = req.EndDate,
           Location = req.Location,
           MaxAttendees = req.MaxAttendees,
           CurrentAttendees = 0,
           IsActive = true,
           CreatedAt = DateTime.UtcNow
       };
       
       await _events.AddAsync(ev);
       await _uow.CommitAsync();

       await _auditService.LogGenericActionAsync(
           "EVENT_CREATED",
           "Event",
           ev.Id.ToString(),
           $"Evento '{req.Title}' creado",
           currentUserId,
           newValues: new { 
               Title = req.Title, 
               Description = req.Description,
               StartDate = req.StartDate,
               EndDate = req.EndDate,
               Location = req.Location,
               MaxAttendees = req.MaxAttendees,
               IsActive = true 
           }
       );

       return Ok(ev);
   }

   [HttpGet]
   public async Task<IActionResult> List()
   {
       var list = await _events.ListAllAsync();
       return Ok(list);
   }

   [HttpGet("{id:int}")]
   public async Task<IActionResult> GetById(int id)
   {
       var ev = await _events.GetByIdAsync(id);
       if (ev == null)
           return NotFound($"Evento con ID {id} no encontrado");

       return Ok(ev);
   }

   [HttpPut("{id:int}")]
   public async Task<IActionResult> Update(int id, [FromBody] UpdateEventRequest req)
   {
       var currentUserId = GetCurrentUserId();
       var ev = await _events.GetByIdAsync(id);
       
       if (ev == null)
           return NotFound($"Evento con ID {id} no encontrado");

       // Validaciones
       if (req.StartDate >= req.EndDate)
           return BadRequest("La fecha de inicio debe ser anterior a la fecha de fin");
           
       if (req.MaxAttendees < ev.CurrentAttendees)
           return BadRequest($"El máximo de asistentes no puede ser menor a los asistentes actuales ({ev.CurrentAttendees})");

       // Guardar valores anteriores para auditoría
       var oldValues = new { 
           Title = ev.Title, 
           Description = ev.Description,
           StartDate = ev.StartDate,
           EndDate = ev.EndDate,
           Location = ev.Location,
           MaxAttendees = ev.MaxAttendees
       };

       // Actualizar valores
       ev.Title = req.Title;
       ev.Description = req.Description;
       ev.StartDate = req.StartDate;
       ev.EndDate = req.EndDate;
       ev.Location = req.Location;
       ev.MaxAttendees = req.MaxAttendees;
       ev.UpdatedAt = DateTime.UtcNow;

       await _events.UpdateAsync(ev);
       await _uow.CommitAsync();

       await _auditService.LogGenericActionAsync(
           "EVENT_UPDATED",
           "Event",
           id.ToString(),
           $"Evento '{req.Title}' actualizado",
           currentUserId,
           oldValues: oldValues,
           newValues: new { 
               Title = req.Title, 
               Description = req.Description,
               StartDate = req.StartDate,
               EndDate = req.EndDate,
               Location = req.Location,
               MaxAttendees = req.MaxAttendees
           }
       );

       return Ok(ev);
   }

   [HttpDelete("{id:int}")]
   public async Task<IActionResult> Delete(int id)
   {
       var currentUserId = GetCurrentUserId();
       var ev = await _events.GetByIdAsync(id);
       
       if (ev == null)
           return NotFound($"Evento con ID {id} no encontrado");

       // Verificar si tiene tickets asociados
       if (ev.CurrentAttendees > 0)
           return BadRequest("No se puede eliminar un evento que tiene asistentes registrados");

       var eventData = new { 
           Title = ev.Title, 
           Description = ev.Description,
           StartDate = ev.StartDate,
           EndDate = ev.EndDate,
           Location = ev.Location,
           MaxAttendees = ev.MaxAttendees,
           CurrentAttendees = ev.CurrentAttendees,
           IsActive = ev.IsActive
       };

       await _events.DeleteAsync(id);
       await _uow.CommitAsync();

       await _auditService.LogGenericActionAsync(
           "EVENT_DELETED",
           "Event",
           id.ToString(),
           $"Evento '{ev.Title}' eliminado",
           currentUserId,
           oldValues: eventData
       );

       return Ok(new { message = "Evento eliminado exitosamente" });
   }
}