using Modules.Users.Domain;
using Modules.Security.Domain;
using Modules.Security.Application;
using Shared.Application;

namespace Modules.Security.Application.Auth;

public interface IAuthAppService
{
    Task<(string token, User user, List<string> roles, List<string> permissions)> LoginAsync(string email, string password);
}

public class AuthAppService : IAuthAppService
{
    private readonly IUserRepository _users;
    private readonly IRoleRepository _roles;
    private readonly IUserRoleRepository _userRoles;
    private readonly IPermissionRepository _permissions;   
    private readonly IJwtTokenService _jwt;
    private readonly IUnitOfWork _uow;

    public AuthAppService(
        IUserRepository users,
        IRoleRepository roles,
        IUserRoleRepository userRoles,
        IPermissionRepository permissions,   
        IJwtTokenService jwt,
        IUnitOfWork uow)
    {
        _users = users;
        _roles = roles;
        _userRoles = userRoles;
        _permissions = permissions;          
        _jwt = jwt;
        _uow = uow;
    }

    public async Task<(string token, User user, List<string> roles, List<string> permissions)> LoginAsync(string email, string password)
    {
        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
            throw new ArgumentException("Email y contraseña son requeridos");

        var e = email.Trim();

        var u = await _users.GetByEmailAsync(e)
            ?? throw new UnauthorizedAccessException("Usuario no encontrado");

        if (!u.IsActive)
            throw new UnauthorizedAccessException("Usuario inactivo");

        // ✅ Verificar contraseña con BCrypt
        if (!BCrypt.Net.BCrypt.Verify(password, u.PasswordHash))
            throw new UnauthorizedAccessException("Credenciales inválidas");

        // Obtener roles asignados al usuario
        var roleNames = await _userRoles.ListRoleNamesForUserAsync(u.Id);
        if (roleNames.Count == 0)
            throw new UnauthorizedAccessException("El usuario no tiene roles asignados");

        // Obtener permisos según los roles
        var permissions = new List<string>();
        foreach (var roleName in roleNames)
        {
            var role = await _roles.GetByNameAsync(roleName);
            if (role != null)
            {
                var perms = await _permissions.ListKeysForRoleAsync(role.Id); 
                permissions.AddRange(perms);
            }
        }

        // Quitar duplicados
        permissions = permissions.Distinct().ToList();

        // Generar token JWT con roles + permisos
        var token = _jwt.Issue(u.Id, u.Name, roleNames, permissions);

        return (token, u, roleNames, permissions);
    }
}
