namespace Modules.Auditing.Domain;

public class AuditLog
{
    public long Id { get; set; }
    public string Action { get; set; } = null!;        
    public string EntityType { get; set; } = null!;    
    public string EntityId { get; set; } = null!;      
    public string Description { get; set; } = null!;   
    public string? OldValues { get; set; }             
    public string? NewValues { get; set; }             
    public int UserId { get; set; }                    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public string? IpAddress { get; set; }
    public string? UserAgent { get; set; }
}