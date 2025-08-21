using Microsoft.AspNetCore.Mvc;
using Modules.Security.Domain;
using Modules.Security.Application;
using Modules.Auditing.Application;
using Shared.Application;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace Modules.Security.Controllers;

[Authorize(Policy = "MANAGE_ROLES")]
[ApiController]
[Route("api/security/roles/{roleId:int}/permissions")] 
public class RolePermissionsController : ControllerBase
{
    private readonly IRoleRepository _roles;
    private readonly IRolePermissionRepository _rolePermissions;
    private readonly IPermissionRepository _permissions;
    private readonly IUnitOfWork _uow;
    private readonly IAuditService _auditService;

    public RolePermissionsController(
        IRoleRepository roles, 
        IRolePermissionRepository rolePermissions, 
        IPermissionRepository permissions,
        IUnitOfWork uow, 
        IAuditService auditService)
    {
        _roles = roles;
        _rolePermissions = rolePermissions;
        _permissions = permissions;
        _uow = uow;
        _auditService = auditService;
    }

    private int GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return int.TryParse(userIdClaim, out var userId) ? userId : 0;
    }

    [HttpGet] 
    public async Task<IActionResult> GetRolePermissions(int roleId)
    {
        var role = await _roles.GetByIdAsync(roleId);
        if (role == null)
            return NotFound($"Rol con ID {roleId} no encontrado");

        var permissions = await _permissions.GetPermissionsForRoleAsync(roleId);

        return Ok(new {
            role = new { role.Id, role.Name },
            permissions = permissions.Select(p => new {
                p.Id,
                p.Key,
                p.Name,
                p.Description,
                p.IsActive
            }),
            total = permissions.Count
        });
    }

    [HttpPost]
    public async Task<IActionResult> AssignPermissions(int roleId, [FromBody] List<int> permissionIds)
    {
        var currentUserId = GetCurrentUserId();
        var role = await _roles.GetByIdAsync(roleId);
        if (role is null)
            return NotFound("Rol no encontrado");

        if (permissionIds == null || !permissionIds.Any())
            return BadRequest("Debe proporcionar al menos un ID de permiso");

        var assignedCount = 0;
        var alreadyAssignedCount = 0;
        var assignedPermissions = new List<object>();

        foreach (var pid in permissionIds)
        {
            var permission = await _permissions.GetByIdAsync(pid);
            if (permission == null) continue;

            var alreadyAssigned = await _permissions.RoleHasPermissionAsync(roleId, pid);
            if (alreadyAssigned)
            {
                alreadyAssignedCount++;
                continue;
            }

            await _rolePermissions.AssignAsync(roleId, pid);
            assignedCount++;
            assignedPermissions.Add(new { 
                permissionId = pid, 
                permission.Name, 
                permission.Key 
            });

            await _auditService.LogGenericActionAsync(
                "PERMISSION_ASSIGNED_TO_ROLE",
                "Role",
                roleId.ToString(),
                $"Permiso '{permission.Name}' asignado al rol '{role.Name}'",
                currentUserId,
                newValues: new { 
                    RoleId = roleId, 
                    RoleName = role.Name,
                    PermissionId = pid,
                    PermissionName = permission.Name,
                    PermissionKey = permission.Key
                }
            );
        }

        if (assignedCount > 0)
        {
            await _uow.CommitAsync();

            await _auditService.LogGenericActionAsync(
                "BULK_PERMISSIONS_ASSIGNED",
                "Role",
                roleId.ToString(),
                $"{assignedCount} permisos asignados al rol '{role.Name}'",
                currentUserId,
                newValues: new { 
                    RoleId = roleId, 
                    RoleName = role.Name,
                    PermissionIds = permissionIds,
                    AssignedCount = assignedCount,
                    AlreadyAssignedCount = alreadyAssignedCount,
                    AssignedPermissions = assignedPermissions
                }
            );
        }

        return Ok(new { 
            message = $"Procesado: {assignedCount} asignados, {alreadyAssignedCount} ya existían",
            roleId, 
            role = new { role.Id, role.Name },
            assignedCount,
            alreadyAssignedCount,
            assignedPermissions
        });
    }

    [HttpDelete]
    public async Task<IActionResult> RemovePermissions(int roleId, [FromBody] List<int> permissionIds)
    {
        var currentUserId = GetCurrentUserId();
        var role = await _roles.GetByIdAsync(roleId);
        if (role == null)
            return NotFound("Rol no encontrado");

        if (permissionIds == null || !permissionIds.Any())
            return BadRequest("Debe proporcionar al menos un ID de permiso");

        var removedCount = 0;
        var notAssignedCount = 0;
        var removedPermissions = new List<object>();

        foreach (var pid in permissionIds)
        {
            var permission = await _permissions.GetByIdAsync(pid);
            if (permission == null) continue;

            var isAssigned = await _permissions.RoleHasPermissionAsync(roleId, pid);
            if (!isAssigned)
            {
                notAssignedCount++;
                continue;
            }

            await _rolePermissions.RemoveAsync(roleId, pid);
            removedCount++;
            removedPermissions.Add(new { 
                permissionId = pid, 
                permission.Name, 
                permission.Key 
            });

            await _auditService.LogGenericActionAsync(
                "PERMISSION_REMOVED_FROM_ROLE",
                "Role",
                roleId.ToString(),
                $"Permiso '{permission.Name}' removido del rol '{role.Name}'",
                currentUserId,
                oldValues: new { 
                    RoleId = roleId, 
                    RoleName = role.Name,
                    PermissionId = pid,
                    PermissionName = permission.Name,
                    PermissionKey = permission.Key
                }
            );
        }

        if (removedCount > 0)
        {
            await _uow.CommitAsync();

            await _auditService.LogGenericActionAsync(
                "BULK_PERMISSIONS_REMOVED",
                "Role",
                roleId.ToString(),
                $"{removedCount} permisos removidos del rol '{role.Name}'",
                currentUserId,
                oldValues: new { 
                    RoleId = roleId, 
                    RoleName = role.Name,
                    PermissionIds = permissionIds,
                    RemovedCount = removedCount,
                    NotAssignedCount = notAssignedCount,
                    RemovedPermissions = removedPermissions
                }
            );
        }

        return Ok(new { 
            message = $"Procesado: {removedCount} removidos, {notAssignedCount} no estaban asignados",
            roleId,
            role = new { role.Id, role.Name },
            removedCount,
            notAssignedCount,
            removedPermissions
        });
    }
}