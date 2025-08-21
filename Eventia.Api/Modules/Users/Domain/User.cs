namespace Modules.Users.Domain;

using Modules.Security.Domain;

public class User
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;

    public int? RoleId { get; set; }
    public Role? Role { get; set; }
}

public interface IUserRepository
{
    Task<User?> GetByIdAsync(int id);      
    Task<User?> GetByEmailAsync(string email);
    Task AddAsync(User user);
    Task SaveAsync(User user);            
    Task DeleteAsync(User user);          
    Task<List<User>> ListAsync();
}
