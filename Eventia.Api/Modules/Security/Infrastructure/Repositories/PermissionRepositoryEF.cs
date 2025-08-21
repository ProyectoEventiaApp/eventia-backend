using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Modules.Security.Domain;

public class PermissionRepositoryEF : IPermissionRepository
{
    private readonly AppDbContext _db;
    public PermissionRepositoryEF(AppDbContext db) => _db = db;

    public Task<List<Permission>> ListForRoleIdsAsync(IEnumerable<int> roleIds)
    {
        var q =
            from rp in _db.RolePermissions
            join p in _db.Permissions on rp.PermissionId equals p.Id
            where roleIds.Contains(rp.RoleId) && p.IsActive
            select p;

        return q.Distinct().ToListAsync();
    }

    public Task<Permission?> GetByKeyAsync(string key) =>
        _db.Permissions.FirstOrDefaultAsync(p => p.Key == key);

    public Task AddAsync(Permission p)
    {
        _db.Permissions.Add(p);
        return Task.CompletedTask;
    }

    public async Task<List<Permission>> ListAllAsync() =>
        await _db.Permissions
            .OrderBy(p => p.Name)
            .ToListAsync();

    public async Task AssignToRoleAsync(int roleId, int permissionId)
    {
        var exists = await _db.RolePermissions
            .AnyAsync(x => x.RoleId == roleId && x.PermissionId == permissionId);
        if (!exists)
            _db.RolePermissions.Add(new RolePermission { RoleId = roleId, PermissionId = permissionId });
    }

    public async Task<List<string>> ListKeysForRoleAsync(int roleId)
    {
        return await _db.RolePermissions
            .Where(rp => rp.RoleId == roleId)
            .Include(rp => rp.Permission)
            .Select(rp => rp.Permission.Key)
            .ToListAsync();
    }


    // GET by ID
    public async Task<Permission?> GetByIdAsync(int id) =>
        await _db.Permissions.FirstOrDefaultAsync(p => p.Id == id);

    // UPDATE
    public Task UpdateAsync(Permission permission)
    {
        _db.Permissions.Update(permission);
        return Task.CompletedTask;
    }

    // DELETE
    public async Task DeleteAsync(int id)
    {
        var permission = await GetByIdAsync(id);
        if (permission != null)
        {
            _db.Permissions.Remove(permission);
        }
    }

    
    // Obtener roles que usan este permiso
    public async Task<List<Role>> GetRolesUsingPermissionAsync(int permissionId)
    {
        return await _db.RolePermissions
            .Where(rp => rp.PermissionId == permissionId)
            .Include(rp => rp.Role)
            .Select(rp => rp.Role)
            .ToListAsync();
    }

    // Eliminar todas las relaciones RolePermission de un permiso
    public async Task DeleteRolePermissionsByPermissionIdAsync(int permissionId)
    {
        var rolePermissions = await _db.RolePermissions
            .Where(rp => rp.PermissionId == permissionId)
            .ToListAsync();

        if (rolePermissions.Any())
        {
            _db.RolePermissions.RemoveRange(rolePermissions);
        }
    }

    public async Task RemoveFromRoleAsync(int roleId, int permissionId)
    {
        var rolePermission = await _db.RolePermissions
            .FirstOrDefaultAsync(rp => rp.RoleId == roleId && rp.PermissionId == permissionId);

        if (rolePermission != null)
        {
            _db.RolePermissions.Remove(rolePermission);
        }
    }

    // ✨ MÉTODO ADICIONAL: Obtener permisos de un rol específico
    public async Task<List<Permission>> GetPermissionsForRoleAsync(int roleId)
    {
        return await _db.RolePermissions
            .Where(rp => rp.RoleId == roleId)
            .Include(rp => rp.Permission)
            .Select(rp => rp.Permission)
            .OrderBy(p => p.Name)
            .ToListAsync();
    }

    public async Task<bool> RoleHasPermissionAsync(int roleId, int permissionId)
    {
        return await _db.RolePermissions
            .AnyAsync(rp => rp.RoleId == roleId && rp.PermissionId == permissionId);
    }

    public async Task<int> CountRolesUsingPermissionAsync(int permissionId)
    {
        return await _db.RolePermissions
            .Where(rp => rp.PermissionId == permissionId)
            .CountAsync();
    }
}