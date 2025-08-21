using Microsoft.EntityFrameworkCore;
using Modules.Security.Domain;
using Persistence;

namespace Modules.Security.Infrastructure.Repositories;

public class RoleRepositoryEF : IRoleRepository
{
    private readonly AppDbContext _db;
    public RoleRepositoryEF(AppDbContext db) => _db = db;

    public Task<Role?> GetByIdAsync(int id) =>
        _db.Roles.FirstOrDefaultAsync(r => r.Id == id);

    public Task<Role?> GetByNameAsync(string name) =>
        _db.Roles.FirstOrDefaultAsync(r => r.Name == name);

    public Task<List<Role>> ListAsync() =>
        _db.Roles.AsNoTracking().ToListAsync();

    public Task AddAsync(Role role)
    {
        _db.Roles.Add(role);
        return Task.CompletedTask;
    }

    public Task UpdateAsync(Role role)
    {
        _db.Roles.Update(role);
        return Task.CompletedTask;
    }
}