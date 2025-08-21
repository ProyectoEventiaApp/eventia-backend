using Microsoft.EntityFrameworkCore;
using Modules.Security.Domain;
using Modules.Users.Domain;
using Persistence;

namespace Modules.Security.Infrastructure.Repositories;

public class UserRoleRepositoryEF : IUserRoleRepository
{
    private readonly AppDbContext _db;
    public UserRoleRepositoryEF(AppDbContext db) => _db = db;

    public async Task AssignAsync(int userId, int roleId)
    {
        var user = await _db.Users.FirstOrDefaultAsync(u => u.Id == userId);
        if (user is null)
            throw new InvalidOperationException($"Usuario con ID {userId} no encontrado.");

        user.RoleId = roleId;
        await _db.SaveChangesAsync();
    }

    public async Task UnassignAsync(int userId, int roleId)
    {
        var user = await _db.Users.FirstOrDefaultAsync(u => u.Id == userId && u.RoleId == roleId);
        if (user is not null)
        {
            user.RoleId = null; 
            await _db.SaveChangesAsync();
        }
    }

    // Obtiene el nombre del rol del usuario
    public async Task<List<string>> ListRoleNamesForUserAsync(int userId)
    {
        var role = await (from u in _db.Users
                          join r in _db.Roles on u.RoleId equals r.Id
                          where u.Id == userId && r.IsActive
                          select r.Name).FirstOrDefaultAsync();

        return role is null ? new List<string>() : new List<string> { role };
    }

    // Obtiene los usuarios que pertenecen a un rol
    public async Task<List<User>> ListUsersForRoleAsync(int roleId)
    {
        return await _db.Users
            .Where(u => u.RoleId == roleId)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<List<int>> ListRoleIdsForUserAsync(int userId)
    {
        var roleId = await _db.Users
            .Where(u => u.Id == userId && u.RoleId != null)
            .Select(u => u.RoleId!.Value)
            .FirstOrDefaultAsync();

        return roleId == 0 ? new List<int>() : new List<int> { roleId };
    }
}
