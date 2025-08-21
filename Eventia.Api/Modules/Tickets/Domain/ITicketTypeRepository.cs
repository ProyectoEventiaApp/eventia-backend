namespace Modules.Tickets.Domain;

public interface ITicketTypeRepository
{
    Task<TicketType?> GetByIdAsync(int id);
    Task<TicketType?> GetByNameAsync(string name);
    Task<IEnumerable<TicketType>> ListAllAsync();
    Task AddAsync(TicketType type);
    void Remove(TicketType type);
}
