using Modules.Users.Domain;
using Modules.Security.Domain;  
using Shared.Application;

namespace Modules.Users.Application;

public interface IUsersAppService
{
    Task<User> CreateAsync(string name, string email, string password); 
    Task AssignRoleAsync(string email, int roleId);                      
    Task UnassignRoleAsync(string email);                               
    Task<List<User>> ListAsync();
    Task<User?> GetByEmailAsync(string email);
    Task<User?> GetByIdAsync(int id);                                    
    Task UpdateAsync(string email, string? name);
    Task UpdateByIdAsync(int id, string? name);                          
    Task DisableAsync(string email);
    Task EnableAsync(string email);                                     
    Task DeleteAsync(int id);                                            
}

public class UsersAppService : IUsersAppService
{
    private readonly IUserRepository _users;
    private readonly IRoleRepository _roles;
    private readonly IUnitOfWork _uow;

    public UsersAppService(IUserRepository users, IRoleRepository roles, IUnitOfWork uow)
    {
        _users = users;
        _roles = roles;
        _uow = uow;
    }

    public async Task<User> CreateAsync(string name, string email, string password)
    {
        var existing = await _users.GetByEmailAsync(email.Trim());
        if (existing is not null)
            throw new InvalidOperationException("Ya existe un usuario con ese correo");

        var u = new User
        {
            Name = name.Trim(),
            Email = email.Trim(),
            IsActive = true,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(password),
            RoleId = null
        };

        await _users.AddAsync(u);
        await _uow.CommitAsync();
        return u;
    }

    public async Task AssignRoleAsync(string email, int roleId)
    {
        var u = await _users.GetByEmailAsync(email.Trim())
                ?? throw new KeyNotFoundException("Usuario no encontrado");

        var r = await _roles.GetByIdAsync(roleId)
                ?? throw new KeyNotFoundException("Rol no encontrado");

        u.RoleId = r.Id;
        await _users.SaveAsync(u);
        await _uow.CommitAsync();
    }

    public async Task UnassignRoleAsync(string email)
    {
        var u = await _users.GetByEmailAsync(email.Trim())
                ?? throw new KeyNotFoundException("Usuario no encontrado");

        u.RoleId = null;
        await _users.SaveAsync(u);
        await _uow.CommitAsync();
    }

    public Task<List<User>> ListAsync() => _users.ListAsync();

    public Task<User?> GetByEmailAsync(string email) => _users.GetByEmailAsync(email.Trim());

    public Task<User?> GetByIdAsync(int id) => _users.GetByIdAsync(id);

    public async Task UpdateAsync(string email, string? name)
    {
        var u = await _users.GetByEmailAsync(email.Trim())
                ?? throw new KeyNotFoundException("Usuario no encontrado");

        if (!string.IsNullOrWhiteSpace(name)) u.Name = name.Trim();

        await _users.SaveAsync(u);
        await _uow.CommitAsync();
    }

    public async Task UpdateByIdAsync(int id, string? name)
    {
        var u = await _users.GetByIdAsync(id)
                ?? throw new KeyNotFoundException("Usuario no encontrado");

        if (!string.IsNullOrWhiteSpace(name)) u.Name = name.Trim();

        await _users.SaveAsync(u);
        await _uow.CommitAsync();
    }

    public async Task DisableAsync(string email)
    {
        var u = await _users.GetByEmailAsync(email.Trim())
                ?? throw new KeyNotFoundException("Usuario no encontrado");

        u.IsActive = false;
        await _users.SaveAsync(u);
        await _uow.CommitAsync();
    }

    public async Task EnableAsync(string email)
    {
        var u = await _users.GetByEmailAsync(email.Trim())
                ?? throw new KeyNotFoundException("Usuario no encontrado");

        u.IsActive = true;
        await _users.SaveAsync(u);
        await _uow.CommitAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var u = await _users.GetByIdAsync(id)
                ?? throw new KeyNotFoundException("Usuario no encontrado");

        await _users.DeleteAsync(u);
        await _uow.CommitAsync();
    }
}