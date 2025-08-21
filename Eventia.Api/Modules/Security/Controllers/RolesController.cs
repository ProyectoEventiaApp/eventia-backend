using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Modules.Security.Domain;
using Modules.Users.Application;
using Modules.Auditing.Application;
using Shared.Application;
using System.Security.Claims;

namespace Modules.Security.Controllers;

[Authorize(Policy = "MANAGE_ROLES")]
[ApiController]
[Route("api/security")]
public class RolesController : ControllerBase
{
    private readonly IRoleRepository _roles;
    private readonly IUserRoleRepository _userRoles;
    private readonly IUsersAppService _users;
    private readonly IUnitOfWork _uow;
    private readonly IAuditService _auditService;

    public RolesController(
        IRoleRepository roles,
        IUserRoleRepository userRoles,
        IUsersAppService users,
        IUnitOfWork uow,
        IAuditService auditService)
    {
        _roles = roles;
        _userRoles = userRoles;
        _users = users;
        _uow = uow;
        _auditService = auditService;
    }

 private int GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return int.TryParse(userIdClaim, out var userId) ? userId : 0;
    }

    // Crear un rol
    [HttpPost("roles/{name}")]
    public async Task<IActionResult> CreateRole(string name)
    {
        var currentUserId = GetCurrentUserId();
        name = name.Trim();
        var r = await _roles.GetByNameAsync(name);
        if (r is not null) return Conflict("Ya existe el rol");

        var newRole = new Role 
        { 
            Name = name, 
            IsActive = true
        };
        
        await _roles.AddAsync(newRole);
        await _uow.CommitAsync();

        await _auditService.LogGenericActionAsync(
            "ROLE_CREATED",
            "Role",
            newRole.Id.ToString(),
            $"Rol '{name}' creado",
            currentUserId,
            newValues: new
            {
                Name = name,
                IsActive = true
            }
        );

        return Ok(new RoleDto
        {
            Id = newRole.Id,
            Name = newRole.Name,
            IsActive = newRole.IsActive
        });
    }

    [HttpGet("roles/{id:int}")]
    public async Task<ActionResult<RoleDto>> GetRoleById(int id)
    {
        var role = await _roles.GetByIdAsync(id);
        if (role is null) return NotFound("Rol no encontrado");

        var roleDto = new RoleDto
        {
            Id = role.Id,
            Name = role.Name,
            IsActive = role.IsActive
        };

        return Ok(roleDto);
    }

    [HttpPut("roles/{id:int}")]
    public async Task<IActionResult> UpdateRole(int id, [FromBody] UpdateRoleRequest request)
    {
        var currentUserId = GetCurrentUserId();
        var role = await _roles.GetByIdAsync(id);
        if (role is null) return NotFound("Rol no encontrado");

        // Verificar si el nuevo nombre ya existe en otro rol
        if (!string.IsNullOrEmpty(request.Name) && request.Name.Trim() != role.Name)
        {
            var existingRole = await _roles.GetByNameAsync(request.Name.Trim());
            if (existingRole is not null && existingRole.Id != id)
                return Conflict("Ya existe otro rol con ese nombre");
        }

        var oldValues = new
        {
            Name = role.Name,
            IsActive = role.IsActive
        };

        if (!string.IsNullOrEmpty(request.Name))
            role.Name = request.Name.Trim();

        if (request.IsActive.HasValue)
            role.IsActive = request.IsActive.Value;

        await _roles.UpdateAsync(role);
        await _uow.CommitAsync();

        await _auditService.LogGenericActionAsync(
            "ROLE_UPDATED",
            "Role",
            role.Id.ToString(),
            $"Rol '{role.Name}' actualizado",
            currentUserId,
            oldValues: oldValues,
            newValues: new
            {
                Name = role.Name,
                IsActive = role.IsActive
            }
        );

        return Ok(new RoleDto
        {
            Id = role.Id,
            Name = role.Name,
            IsActive = role.IsActive
        });
    }

    [HttpGet("roles")]
    public async Task<ActionResult<List<RoleDto>>> ListRoles()
    {
        var roles = await _roles.ListAsync();
        var roleDtos = roles.Select(r => new RoleDto
        {
            Id = r.Id,
            Name = r.Name,
            IsActive = r.IsActive
        }).ToList();

        return Ok(roleDtos);
    }

    // Listar usuarios de un rol
    [HttpGet("roles/{roleName}/users")]
    public async Task<ActionResult<List<RoleUserDto>>> ListUsersByRole(string roleName)  
    {
        var currentUserId = GetCurrentUserId();
        var role = await _roles.GetByNameAsync(roleName.Trim());
        if (role is null) return NotFound("Rol no existe");

        var users = await _userRoles.ListUsersForRoleAsync(role.Id);

        await _auditService.LogGenericActionAsync(
            "ROLE_USERS_QUERIED",
            "Role",
            role.Id.ToString(),
            $"Consultados usuarios del rol '{roleName}' ({users.Count()} usuarios encontrados)",
            currentUserId,
            newValues: new
            {
                RoleName = roleName,
                UserCount = users.Count(),
                QueryTime = DateTime.UtcNow
            }
        );

        var userDtos = users.Select(u => new UserDto
        {
            Id = u.Id,
            Name = u.Name,  
            Email = u.Email,
            IsActive = u.IsActive
        }).ToList();

        return Ok(userDtos);
    }
}

public class RoleUserDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public bool IsActive { get; set; }
}

public class RoleDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public bool IsActive { get; set; }
}

public class UserDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;  
    public string Email { get; set; } = string.Empty;
    public bool IsActive { get; set; }
}

public class UpdateRoleRequest
{
    public string? Name { get; set; }
    public bool? IsActive { get; set; }
}