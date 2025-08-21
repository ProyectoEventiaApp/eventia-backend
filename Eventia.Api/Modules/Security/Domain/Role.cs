using Modules.Users.Domain;

namespace Modules.Security.Domain;

public class Role
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;

    public ICollection<User> Users { get; set; } = new List<User>();

    public ICollection<RolePermission> RolePermissions { get; set; } = new List<RolePermission>();
}
