namespace Modules.Events.Domain;

public interface IEventRepository
{
    Task<Event?> GetByIdAsync(int id);
    Task<List<Event>> ListAllAsync();
    Task AddAsync(Event ev);
    
    Task UpdateAsync(Event ev);
    Task DeleteAsync(int id);
}