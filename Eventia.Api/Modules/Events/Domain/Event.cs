// Modules/Events/Domain/Event.cs - Modelo actualizado
using Modules.Tickets.Domain;

namespace Modules.Events.Domain;

public class Event
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public string Location { get; set; } = string.Empty;
    public int MaxAttendees { get; set; } = 0;
    public int CurrentAttendees { get; set; } = 0;
    
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    // Relaciones
    public ICollection<Ticket> Tickets { get; set; } = new List<Ticket>();
}