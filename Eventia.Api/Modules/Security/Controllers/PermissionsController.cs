using Microsoft.AspNetCore.Mvc;
using Modules.Security.Domain;
using Modules.Security.Application;
using Modules.Auditing.Application;
using Shared.Application;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace Modules.Security.Controllers;

[Authorize(Policy = "MANAGE_PERMISSIONS")]
[ApiController]
[Route("api/security/permissions")]
public class PermissionsController : ControllerBase
{
    private readonly IPermissionRepository _permissions;
    private readonly IUnitOfWork _uow;
    private readonly IAuditService _auditService;

    public PermissionsController(IPermissionRepository permissions, IUnitOfWork uow, IAuditService auditService)
    {
        _permissions = permissions;
        _uow = uow;
        _auditService = auditService;
    }

    private int GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return int.TryParse(userIdClaim, out var userId) ? userId : 0;
    }

    public record CreatePermissionRequest(string Key, string Name, string Description);
    public record UpdatePermissionRequest(string Name, string Description, bool IsActive);

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var permissions = await _permissions.ListAllAsync();
        return Ok(permissions);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var permission = await _permissions.GetByIdAsync(id);
        if (permission == null)
            return NotFound($"Permiso con ID {id} no encontrado");

        return Ok(permission);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreatePermissionRequest req)
    {
        var currentUserId = GetCurrentUserId();

        var existingPermission = await _permissions.GetByKeyAsync(req.Key);
        if (existingPermission != null)
            return BadRequest($"Ya existe un permiso con la clave '{req.Key}'");

        var permission = new Permission
        {
            Key = req.Key.ToUpper(), 
            Name = req.Name,
            Description = req.Description,
            IsActive = true
        };

        await _permissions.AddAsync(permission);
        await _uow.CommitAsync();

        await _auditService.LogGenericActionAsync(
            "PERMISSION_CREATED",
            "Permission",
            permission.Id.ToString(),
            $"Permiso '{req.Name}' creado con clave '{req.Key}'",
            currentUserId,
            newValues: new { 
                Key = req.Key, 
                Name = req.Name, 
                Description = req.Description,
                IsActive = true 
            }
        );

        return Ok(permission);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdatePermissionRequest req)
    {
        var currentUserId = GetCurrentUserId();
        var permission = await _permissions.GetByIdAsync(id);
        
        if (permission == null)
            return NotFound($"Permiso con ID {id} no encontrado");

        var oldValues = new { 
            Name = permission.Name, 
            Description = permission.Description,
            IsActive = permission.IsActive
        };

        permission.Name = req.Name;
        permission.Description = req.Description;
        permission.IsActive = req.IsActive;

        await _permissions.UpdateAsync(permission);
        await _uow.CommitAsync();

        await _auditService.LogGenericActionAsync(
            "PERMISSION_UPDATED",
            "Permission",
            id.ToString(),
            $"Permiso '{req.Name}' actualizado",
            currentUserId,
            oldValues: oldValues,
            newValues: new { 
                Name = req.Name, 
                Description = req.Description,
                IsActive = req.IsActive
            }
        );

        return Ok(permission);
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var currentUserId = GetCurrentUserId();
        var permission = await _permissions.GetByIdAsync(id);
        
        if (permission == null)
            return NotFound($"Permiso con ID {id} no encontrado");

        var rolesWithPermission = await _permissions.GetRolesUsingPermissionAsync(id);
        var relationshipCount = rolesWithPermission.Count;

        var permissionData = new { 
            Key = permission.Key,
            Name = permission.Name, 
            Description = permission.Description,
            IsActive = permission.IsActive,
            AssignedToRoles = rolesWithPermission.Select(r => new { r.Id, r.Name }).ToList()
        };

        if (relationshipCount > 0)
        {
            await _permissions.DeleteRolePermissionsByPermissionIdAsync(id);
        }

        await _permissions.DeleteAsync(id);
        await _uow.CommitAsync();

        await _auditService.LogGenericActionAsync(
            "PERMISSION_DELETED",
            "Permission",
            id.ToString(),
            $"Permiso '{permission.Name}' eliminado junto con {relationshipCount} asignaciones a roles",
            currentUserId,
            oldValues: permissionData
        );

        return Ok(new { 
            message = "Permiso eliminado exitosamente", 
            relationshipsDeleted = relationshipCount,
            affectedRoles = rolesWithPermission.Select(r => r.Name).ToList()
        });
    }
}