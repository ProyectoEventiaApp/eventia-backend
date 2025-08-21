using Microsoft.EntityFrameworkCore;
using Modules.Security.Domain;
using Persistence;

namespace Modules.Security.Infrastructure;

public class RolePermissionRepository : IRolePermissionRepository
{
    private readonly AppDbContext _db;

    public RolePermissionRepository(AppDbContext db) => _db = db;

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

    public async Task<List<int>> ListPermissionIdsForRoleAsync(int roleId) =>
        await _db.RolePermissions
            .Where(rp => rp.RoleId == roleId)
            .Select(rp => rp.PermissionId)
            .ToListAsync();
}
