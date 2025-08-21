using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Modules.Users.Domain;
using Modules.Security.Domain;
using Shared.Application;
using Modules.Auditing.Application;
using System.Security.Claims;

namespace Modules.Users.Controllers;

[Authorize(Policy = "MANAGE_USERS")]
[ApiController]
[Route("api/users")]
public class UsersController : ControllerBase
{
   private readonly IUserRepository _users;
   private readonly IRoleRepository _roles;
   private readonly IUnitOfWork _uow;
   private readonly IAuditService _auditService;

   public UsersController(IUserRepository users, IRoleRepository roles, IUnitOfWork uow, IAuditService auditService)
   {
       _users = users;
       _roles = roles;
       _uow = uow;
       _auditService = auditService;
   }

   private int GetCurrentUserId()
   {
       var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
       return int.TryParse(userIdClaim, out var userId) ? userId : 0;
   }

   public record CreateUserRequest(string Name, string Email, string Password);
   public record UpdateUserRequest(string? Name);
   public record UserDto(int Id, string Name, string Email, bool IsActive, int? RoleId, string? RoleName);

   [HttpPost]
   public async Task<ActionResult<UserDto>> Create([FromBody] CreateUserRequest req)
   {
       var currentUserId = GetCurrentUserId();
       var email = req.Email.Trim();

       var existing = await _users.GetByEmailAsync(email);
       if (existing is not null) return Conflict("Email ya registrado");

       var u = new User
       {
           Name = req.Name.Trim(),
           Email = email,
           IsActive = true,
           PasswordHash = BCrypt.Net.BCrypt.HashPassword(req.Password),
           RoleId = null
       };

       await _users.AddAsync(u);
       await _uow.CommitAsync();

       await _auditService.LogUserCreatedAsync(u.Id, u.Email, currentUserId);

       return Ok(new UserDto(u.Id, u.Name, u.Email, u.IsActive, u.RoleId, null));
   }

   [HttpGet]
   public async Task<ActionResult<List<UserDto>>> List()
   {
       var list = await _users.ListAsync();
       var result = new List<UserDto>(list.Count);

       foreach (var u in list)
       {
           string? roleName = null;
           if (u.RoleId is int rid)
           {
               var r = await _roles.GetByIdAsync(rid);
               roleName = r?.Name;
           }
           result.Add(new UserDto(u.Id, u.Name, u.Email, u.IsActive, u.RoleId, roleName));
       }

       return Ok(result);
   }

   [HttpGet("{id:int}")]
   public async Task<ActionResult<UserDto>> GetById(int id)
   {
       var u = await _users.GetByIdAsync(id);
       if (u is null) return NotFound("Usuario no encontrado");

       string? roleName = null;
       if (u.RoleId is int rid)
       {
           var r = await _roles.GetByIdAsync(rid);
           roleName = r?.Name;
       }

       return Ok(new UserDto(u.Id, u.Name, u.Email, u.IsActive, u.RoleId, roleName));
   }

   [HttpGet("email/{email}")]
   public async Task<ActionResult<UserDto>> GetByEmail(string email)
   {
       var u = await _users.GetByEmailAsync(email.Trim());
       if (u is null) return NotFound("Usuario no encontrado");

       string? roleName = null;
       if (u.RoleId is int rid)
       {
           var r = await _roles.GetByIdAsync(rid);
           roleName = r?.Name;
       }

       return Ok(new UserDto(u.Id, u.Name, u.Email, u.IsActive, u.RoleId, roleName));
   }

   [HttpPut("{id:int}")]
   public async Task<ActionResult<UserDto>> Update(int id, [FromBody] UpdateUserRequest req)
   {
       var currentUserId = GetCurrentUserId();
       var u = await _users.GetByIdAsync(id);
       if (u is null) return NotFound("Usuario no encontrado");

       // Guardar valores anteriores para auditoría
       var oldValues = new
       {
           Name = u.Name,
           Email = u.Email
       };

       if (!string.IsNullOrWhiteSpace(req.Name))
           u.Name = req.Name.Trim();

       await _users.SaveAsync(u);
       await _uow.CommitAsync();

       await _auditService.LogGenericActionAsync(
           "USER_UPDATED",
           "User",
           u.Id.ToString(),
           $"Usuario {u.Email} actualizado",
           currentUserId,
           oldValues: oldValues,
           newValues: new
           {
               Name = u.Name,
               Email = u.Email
           }
       );

       string? roleName = null;
       if (u.RoleId is int rid)
       {
           var r = await _roles.GetByIdAsync(rid);
           roleName = r?.Name;
       }

       return Ok(new UserDto(u.Id, u.Name, u.Email, u.IsActive, u.RoleId, roleName));
   }

   [HttpDelete("{id:int}")]
   public async Task<IActionResult> Delete(int id)
   {
       var currentUserId = GetCurrentUserId();
       var u = await _users.GetByIdAsync(id);
       if (u is null) return NotFound("Usuario no encontrado");



       await _users.DeleteAsync(u);
       await _uow.CommitAsync();

       await _auditService.LogGenericActionAsync(
           "USER_DELETED",
           "User",
           u.Id.ToString(),
           $"Usuario {u.Email} eliminado",
           currentUserId,
           oldValues: new
           {
               Name = u.Name,
               Email = u.Email,
               IsActive = u.IsActive,
               RoleId = u.RoleId
           },
           newValues: new { Deleted = true }
       );

       return NoContent();
   }

   [HttpPost("{email}/disable")]
   public async Task<IActionResult> Disable(string email)
   {
       var currentUserId = GetCurrentUserId();
       var u = await _users.GetByEmailAsync(email.Trim());
       if (u is null) return NotFound();
       
       u.IsActive = false;
       await _users.SaveAsync(u);
       await _uow.CommitAsync();

       await _auditService.LogGenericActionAsync(
           "USER_DISABLED",
           "User",
           u.Id.ToString(),
           $"Usuario {email} deshabilitado",
           currentUserId,
           oldValues: new { IsActive = true },
           newValues: new { IsActive = false }
       );

       return NoContent();
   }

   [HttpPost("{email}/enable")]
   public async Task<IActionResult> Enable(string email)
   {
       var currentUserId = GetCurrentUserId();
       var u = await _users.GetByEmailAsync(email.Trim());
       if (u is null) return NotFound();
       
       u.IsActive = true;
       await _users.SaveAsync(u);
       await _uow.CommitAsync();

       await _auditService.LogGenericActionAsync(
           "USER_ENABLED",
           "User",
           u.Id.ToString(),
           $"Usuario {email} habilitado",
           currentUserId,
           oldValues: new { IsActive = false },
           newValues: new { IsActive = true }
       );

       return NoContent();
   }

   [HttpPost("{email}/assign-role/{roleId:int}")]
   public async Task<IActionResult> AssignRole(string email, int roleId)
   {
       var currentUserId = GetCurrentUserId();
       var u = await _users.GetByEmailAsync(email.Trim());
       if (u is null) return NotFound("Usuario no encontrado");

       var r = await _roles.GetByIdAsync(roleId);
       if (r is null) return NotFound("Rol no encontrado");

       var oldRoleId = u.RoleId;
       u.RoleId = r.Id;
       await _users.SaveAsync(u);
       await _uow.CommitAsync();

       await _auditService.LogGenericActionAsync(
           "USER_ROLE_ASSIGNED",
           "User",
           u.Id.ToString(),
           $"Rol '{r.Name}' asignado al usuario {email}",
           currentUserId,
           oldValues: new { RoleId = oldRoleId },
           newValues: new { RoleId = r.Id, RoleName = r.Name }
       );

       return Ok(new { u.Email, RoleId = u.RoleId, RoleName = r.Name });
   }

   [HttpDelete("{email}/role")]
   public async Task<IActionResult> RemoveRole(string email)
   {
       var currentUserId = GetCurrentUserId();
       var u = await _users.GetByEmailAsync(email.Trim());
       if (u is null) return NotFound("Usuario no encontrado");

       var oldRoleId = u.RoleId;
       string? oldRoleName = null;
       
       if (oldRoleId is int rid)
       {
           var r = await _roles.GetByIdAsync(rid);
           oldRoleName = r?.Name;
       }

       u.RoleId = null;
       await _users.SaveAsync(u);
       await _uow.CommitAsync();

       await _auditService.LogGenericActionAsync(
           "USER_ROLE_REMOVED",
           "User",
           u.Id.ToString(),
           $"Rol removido del usuario {email}",
           currentUserId,
           oldValues: new { RoleId = oldRoleId, RoleName = oldRoleName },
           newValues: new { RoleId = (int?)null, RoleName = (string?)null }
       );

       return Ok(new { u.Email, RoleId = u.RoleId, RoleName = (string?)null });
   }
}