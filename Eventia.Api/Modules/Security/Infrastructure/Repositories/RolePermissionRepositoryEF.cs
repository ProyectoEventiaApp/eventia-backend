using Microsoft.EntityFrameworkCore;
using Persistence; // tu AppDbContext
using Modules.Security.Domain;

namespace Modules.Security.Infrastructure.Repositories;

public class RolePermissionRepositoryEF : IRolePermissionRepository
{
    private readonly AppDbContext _db;

    public RolePermissionRepositoryEF(AppDbContext db)
    {
        _db = db;
    }

    public async Task AssignAsync(int roleId, int permissionId)
    {
        var exists = await _db.RolePermissions
            .AnyAsync(rp => rp.RoleId == roleId && rp.PermissionId == permissionId);

        if (!exists)
        {
            _db.RolePermissions.Add(new RolePermission
            {
                RoleId = roleId,
                PermissionId = permissionId
            });
        }
    }

    public async Task RemoveAsync(int roleId, int permissionId)
    {
        var entity = await _db.RolePermissions
            .FirstOrDefaultAsync(rp => rp.RoleId == roleId && rp.PermissionId == permissionId);

        if (entity != null)
            _db.RolePermissions.Remove(entity);
    }

    public async Task<List<int>> ListPermissionIdsForRoleAsync(int roleId)
    {
        return await _db.RolePermissions
            .Where(rp => rp.RoleId == roleId)
            .Select(rp => rp.PermissionId)
            .ToListAsync();
    }
}
