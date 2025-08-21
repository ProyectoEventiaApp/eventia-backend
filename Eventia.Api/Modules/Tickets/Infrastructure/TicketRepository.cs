using Modules.Tickets.Domain;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Modules.Tickets.Infrastructure;

public class TicketRepository : ITicketRepository
{
    private readonly AppDbContext _db;

    public TicketRepository(AppDbContext db) => _db = db;

    public async Task<Ticket?> GetByIdAsync(int id) =>
        await _db.Tickets.FirstOrDefaultAsync(t => t.Id == id);

    public async Task<IEnumerable<Ticket>> ListAllAsync() =>
        await _db.Tickets.ToListAsync();

    public async Task AddAsync(Ticket ticket) =>
        await _db.Tickets.AddAsync(ticket);

    public void Update(Ticket ticket) =>
        _db.Tickets.Update(ticket);

    public void Remove(Ticket ticket) =>
        _db.Tickets.Remove(ticket);
}
