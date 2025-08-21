using Modules.Users.Domain;

namespace Modules.Security.Domain;

public interface IUserRoleRepository
{
    Task AssignAsync(int userId, int roleId);
    Task UnassignAsync(int userId, int roleId);
    Task<List<string>> ListRoleNamesForUserAsync(int userId);
    Task<List<User>> ListUsersForRoleAsync(int roleId);
    Task<List<int>> ListRoleIdsForUserAsync(int userId);

}
