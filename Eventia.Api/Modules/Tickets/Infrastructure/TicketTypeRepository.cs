using Modules.Tickets.Domain;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Modules.Tickets.Infrastructure;

public class TicketTypeRepository : ITicketTypeRepository
{
    private readonly AppDbContext _db;

    public TicketTypeRepository(AppDbContext db) => _db = db;

    public async Task<TicketType?> GetByIdAsync(int id) =>
        await _db.TicketTypes.FindAsync(id);

    public async Task<TicketType?> GetByNameAsync(string name) =>
        await _db.TicketTypes.FirstOrDefaultAsync(t => t.Name == name);

    public async Task<IEnumerable<TicketType>> ListAllAsync() =>
        await _db.TicketTypes.ToListAsync();

    public async Task AddAsync(TicketType type) =>
        await _db.TicketTypes.AddAsync(type);

    public void Remove(TicketType type) =>
        _db.TicketTypes.Remove(type);
}
