namespace Modules.Security.Domain;

public interface IRolePermissionRepository
{
    Task AssignAsync(int roleId, int permissionId);
    Task RemoveAsync(int roleId, int permissionId);
    Task<List<int>> ListPermissionIdsForRoleAsync(int roleId);
}
