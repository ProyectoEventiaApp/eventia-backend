namespace Modules.Security.Domain;

public interface IPermissionRepository
{
    // ✅ Métodos existentes
    Task<List<Permission>> ListForRoleIdsAsync(IEnumerable<int> roleIds);
    Task<Permission?> GetByKeyAsync(string key);
    Task AddAsync(Permission p);
    Task<List<Permission>> ListAllAsync();
    Task AssignToRoleAsync(int roleId, int permissionId);
    Task<List<string>> ListKeysForRoleAsync(int roleId);

    Task<Permission?> GetByIdAsync(int id);
    Task UpdateAsync(Permission permission);
    Task DeleteAsync(int id);

    Task<List<Role>> GetRolesUsingPermissionAsync(int permissionId);
    Task DeleteRolePermissionsByPermissionIdAsync(int permissionId);
    Task RemoveFromRoleAsync(int roleId, int permissionId);
    Task<List<Permission>> GetPermissionsForRoleAsync(int roleId);
    Task<bool> RoleHasPermissionAsync(int roleId, int permissionId);
    Task<int> CountRolesUsingPermissionAsync(int permissionId);
}