using System.Text.Json.Serialization;

namespace Modules.Tickets.Domain;

public class Ticket
{
    public int Id { get; set; }
    public string Title { get; set; } = null!;
    public string Description { get; set; } = null!;
    public TicketStatus Status { get; set; } = TicketStatus.Open; // 👈 por default
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    // Evento
    public int EventId { get; set; }

    // Usuario al que se asigna el ticket
    public int? AssignedUserId { get; set; }

    public int CreatedByUserId { get; set; }

    // Tipo de ticket
    public int TicketTypeId { get; set; }

}

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum TicketStatus
{
    [JsonPropertyName("open")]
    Open = 0,

    [JsonPropertyName("in_progress")]
    InProgress = 1,

    [JsonPropertyName("closed")]
    Closed = 2
}
