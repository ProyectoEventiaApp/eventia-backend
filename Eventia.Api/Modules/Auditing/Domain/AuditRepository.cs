namespace Modules.Auditing.Domain;
public interface IAuditRepository
{
    Task AddAsync(AuditLog auditLog);
    Task<IEnumerable<AuditLog>> GetByEntityAsync(string entityType, string entityId);
    Task<IEnumerable<AuditLog>> GetByUserAsync(int userId);
    Task<IEnumerable<AuditLog>> GetRecentAsync(int count = 100);
}