using Microsoft.EntityFrameworkCore;
using Modules.Users.Domain;
using Persistence;

namespace Modules.Users.Infrastructure.Repositories;

public class UserRepositoryEF : IUserRepository
{
    private readonly AppDbContext _db;
    public UserRepositoryEF(AppDbContext db) => _db = db;

    public Task<User?> GetByIdAsync(int id) =>
        _db.Users.FirstOrDefaultAsync(u => u.Id == id);

    public Task<User?> GetByEmailAsync(string email) =>
        _db.Users.FirstOrDefaultAsync(u => u.Email == email);

    public Task<List<User>> ListAsync() =>
        _db.Users.AsNoTracking().ToListAsync();

    public Task AddAsync(User u)
    {
        _db.Users.Add(u);
        return Task.CompletedTask;
    }

    public Task SaveAsync(User u)
    {
        _db.Users.Update(u);
        return Task.CompletedTask;
    }

    public Task DeleteAsync(User u)
    {
        _db.Users.Remove(u);
        return Task.CompletedTask;
    }
}