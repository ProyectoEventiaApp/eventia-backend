using System.Text.Json;
using Modules.Auditing.Domain;
using Shared.Application;
using Modules.Auditing.Application;

namespace Modules.Auditing.Application;

public class AuditService : IAuditService
{
    private readonly IAuditRepository _auditRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public AuditService(IAuditRepository auditRepository, IUnitOfWork unitOfWork, IHttpContextAccessor httpContextAccessor)
    {
        _auditRepository = auditRepository;
        _unitOfWork = unitOfWork;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task LogTicketAssignedAsync(int ticketId, int assignedUserId, int currentUserId)
    {
        var auditLog = new AuditLog
        {
            Action = "TICKET_ASSIGNED",
            EntityType = "Ticket",
            EntityId = ticketId.ToString(),
            Description = $"Ticket #{ticketId} asignado al usuario #{assignedUserId}",
            NewValues = JsonSerializer.Serialize(new { AssignedUserId = assignedUserId }),
            UserId = currentUserId,
            IpAddress = GetClientIpAddress(),
            UserAgent = GetUserAgent()
        };

        await _auditRepository.AddAsync(auditLog);
        await _unitOfWork.CommitAsync();
    }

    public async Task LogTicketStatusChangedAsync(int ticketId, string oldStatus, string newStatus, int currentUserId)
    {
        var auditLog = new AuditLog
        {
            Action = "TICKET_STATUS_CHANGED",
            EntityType = "Ticket",
            EntityId = ticketId.ToString(),
            Description = $"Estado del ticket #{ticketId} cambió de {oldStatus} a {newStatus}",
            OldValues = JsonSerializer.Serialize(new { Status = oldStatus }),
            NewValues = JsonSerializer.Serialize(new { Status = newStatus }),
            UserId = currentUserId,
            IpAddress = GetClientIpAddress(),
            UserAgent = GetUserAgent()
        };

        await _auditRepository.AddAsync(auditLog);
        await _unitOfWork.CommitAsync();
    }

    public async Task LogUserCreatedAsync(int userId, string userEmail, int currentUserId)
    {
        var auditLog = new AuditLog
        {
            Action = "USER_CREATED",
            EntityType = "User",
            EntityId = userId.ToString(),
            Description = $"Usuario {userEmail} creado",
            NewValues = JsonSerializer.Serialize(new { Email = userEmail }),
            UserId = currentUserId,
            IpAddress = GetClientIpAddress(),
            UserAgent = GetUserAgent()
        };

        await _auditRepository.AddAsync(auditLog);
        await _unitOfWork.CommitAsync();
    }

    public async Task LogPermissionAssignedAsync(int roleId, string permission, int currentUserId)
    {
        var auditLog = new AuditLog
        {
            Action = "PERMISSION_ASSIGNED",
            EntityType = "Role",
            EntityId = roleId.ToString(),
            Description = $"Permiso '{permission}' asignado al rol #{roleId}",
            NewValues = JsonSerializer.Serialize(new { Permission = permission }),
            UserId = currentUserId,
            IpAddress = GetClientIpAddress(),
            UserAgent = GetUserAgent()
        };

        await _auditRepository.AddAsync(auditLog);
        await _unitOfWork.CommitAsync();
    }

    private string? GetClientIpAddress()
    {
        var context = _httpContextAccessor.HttpContext;
        return context?.Connection.RemoteIpAddress?.ToString();
    }

    private string? GetUserAgent()
    {
        var context = _httpContextAccessor.HttpContext;
        return context?.Request.Headers["User-Agent"].FirstOrDefault();
    }
    public async Task LogGenericActionAsync(string action, string entityType, string entityId, string description, int currentUserId, object? oldValues = null, object? newValues = null)
    {
        var auditLog = new AuditLog
        {
            Action = action,
            EntityType = entityType,
            EntityId = entityId,
            Description = description,
            OldValues = oldValues != null ? JsonSerializer.Serialize(oldValues) : null,
            NewValues = newValues != null ? JsonSerializer.Serialize(newValues) : null,
            UserId = currentUserId,
            IpAddress = GetClientIpAddress(),
            UserAgent = GetUserAgent()
        };

        await _auditRepository.AddAsync(auditLog);
        await _unitOfWork.CommitAsync();
    }
}