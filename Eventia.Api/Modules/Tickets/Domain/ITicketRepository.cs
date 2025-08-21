namespace Modules.Tickets.Domain;

public interface ITicketRepository
{
    Task<Ticket?> GetByIdAsync(int id);
    Task<IEnumerable<Ticket>> ListAllAsync();
    Task AddAsync(Ticket ticket);
    void Update(Ticket ticket);
    void Remove(Ticket ticket);
}
