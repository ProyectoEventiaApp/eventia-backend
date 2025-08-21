using Modules.Security.Domain;

namespace Modules.Security.Domain;

public interface IRoleRepository
{
    Task<Role?> GetByIdAsync(int id);      
    Task<Role?> GetByNameAsync(string name);
    Task AddAsync(Role role);
    Task UpdateAsync(Role role);  
    Task<List<Role>> ListAsync();
}