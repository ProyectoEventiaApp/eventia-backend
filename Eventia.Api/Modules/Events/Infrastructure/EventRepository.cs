using Microsoft.EntityFrameworkCore;
using Persistence;
using Modules.Events.Domain;

namespace Modules.Events.Infrastructure;

public class EventRepository : IEventRepository
{
    private readonly AppDbContext _db;
    public EventRepository(AppDbContext db) => _db = db;

    public async Task<Event?> GetByIdAsync(int id) =>
        await _db.Events.FirstOrDefaultAsync(e => e.Id == id);

    public async Task<List<Event>> ListAllAsync() =>
        await _db.Events.ToListAsync();

    public async Task AddAsync(Event ev) =>
        await _db.Events.AddAsync(ev);

    public async Task UpdateAsync(Event ev)
    {
        _db.Events.Update(ev);
    }

    public async Task DeleteAsync(int id)
    {
        var eventToDelete = await GetByIdAsync(id);
        if (eventToDelete != null)
        {
            _db.Events.Remove(eventToDelete);
        }
    }
}