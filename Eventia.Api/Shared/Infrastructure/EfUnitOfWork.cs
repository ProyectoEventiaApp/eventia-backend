using Persistence;            // AppDbContext
using Shared.Application;     // IUnitOfWork

namespace Shared.Infrastructure;

public class EfUnitOfWork : IUnitOfWork
{
    private readonly AppDbContext _db;
    public EfUnitOfWork(AppDbContext db) => _db = db;

    public Task CommitAsync() => _db.SaveChangesAsync();
}
