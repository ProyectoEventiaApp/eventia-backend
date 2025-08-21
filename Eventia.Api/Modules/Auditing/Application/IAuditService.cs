namespace Modules.Auditing.Application;

public interface IAuditService
{
    Task LogTicketAssignedAsync(int ticketId, int assignedUserId, int currentUserId);
    Task LogTicketStatusChangedAsync(int ticketId, string oldStatus, string newStatus, int currentUserId);
    Task LogUserCreatedAsync(int userId, string userEmail, int currentUserId);
    Task LogPermissionAssignedAsync(int roleId, string permission, int currentUserId);
    Task LogGenericActionAsync(string action, string entityType, string entityId, string description, int currentUserId, object? oldValues = null, object? newValues = null);
}