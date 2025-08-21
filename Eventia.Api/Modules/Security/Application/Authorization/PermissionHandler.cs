using Microsoft.AspNetCore.Authorization;

namespace Modules.Security.Application.Authorization;

public class PermissionHandler : AuthorizationHandler<PermissionRequirement>
{
    protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, PermissionRequirement requirement)
    {
        // El claim "permissions" se llena al generar el JWT
        var permissions = context.User.FindAll("permissions").Select(c => c.Value);

        if (permissions.Contains(requirement.Permission))
            context.Succeed(requirement);

        return Task.CompletedTask;
    }
}
