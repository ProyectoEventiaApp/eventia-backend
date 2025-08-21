namespace Modules.Tickets.Application;

using Modules.Tickets.Domain;
using Shared.Application;

public interface ITicketAppService
{
    Task<IEnumerable<Ticket>> GetAllAsync();
    Task<Ticket?> GetByIdAsync(int id);
    Task<Ticket> CreateAsync(string title, string description, int eventId, int? assignedUserId, int ticketTypeId, int createdByUserId);
    Task<Ticket> UpdateAsync(int id, string? title, string? description, int? assignedUserId);
    Task<Ticket> ChangeStatusAsync(int id, TicketStatus status);
    Task DeleteAsync(int id);
}

public class TicketAppService : ITicketAppService
{
    private readonly ITicketRepository _repo;
    private readonly IUnitOfWork _uow;

    public TicketAppService(ITicketRepository repo, IUnitOfWork uow)
    {
        _repo = repo; _uow = uow;
    }

    public async Task<IEnumerable<Ticket>> GetAllAsync() => await _repo.ListAllAsync();

    public async Task<Ticket?> GetByIdAsync(int id) => await _repo.GetByIdAsync(id);

    public async Task<Ticket> CreateAsync(string title, string description, int eventId, int? assignedUserId, int ticketTypeId, int createdByUserId)
    {
        var ticket = new Ticket
        {
            Title = title,
            Description = description,
            EventId = eventId,
            AssignedUserId = assignedUserId,
            TicketTypeId = ticketTypeId,
            CreatedByUserId = createdByUserId,
            Status = TicketStatus.Open,
            CreatedAt = DateTime.UtcNow
        };

        await _repo.AddAsync(ticket);
        await _uow.CommitAsync();
        return ticket;
    }

    public async Task<Ticket> UpdateAsync(int id, string? title, string? description, int? assignedUserId)
    {
        var ticket = await _repo.GetByIdAsync(id) ?? throw new KeyNotFoundException("Ticket no encontrado");

        if (!string.IsNullOrWhiteSpace(title)) ticket.Title = title;
        if (!string.IsNullOrWhiteSpace(description)) ticket.Description = description;
        if (assignedUserId.HasValue) ticket.AssignedUserId = assignedUserId.Value;

        ticket.UpdatedAt = DateTime.UtcNow;

        _repo.Update(ticket);
        await _uow.CommitAsync();
        return ticket;
    }

    public async Task<Ticket> ChangeStatusAsync(int id, TicketStatus status)
    {
        var ticket = await _repo.GetByIdAsync(id) ?? throw new KeyNotFoundException("Ticket no encontrado");
        ticket.Status = status;
        ticket.UpdatedAt = DateTime.UtcNow;

        _repo.Update(ticket);
        await _uow.CommitAsync();
        return ticket;
    }

    public async Task DeleteAsync(int id)
    {
        var ticket = await _repo.GetByIdAsync(id) ?? throw new KeyNotFoundException("Ticket no encontrado");
        _repo.Remove(ticket);
        await _uow.CommitAsync();
    }
}
