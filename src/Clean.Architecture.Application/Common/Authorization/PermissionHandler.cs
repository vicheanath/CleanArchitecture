using Microsoft.AspNetCore.Authorization;

namespace Clean.Architecture.Application.Common.Authorization;

/// <summary>
/// Authorization handler for permission-based access control.
/// </summary>
public class PermissionHandler : AuthorizationHandler<PermissionRequirement>
{
    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        PermissionRequirement requirement)
    {
        if (context.User == null || context.User.Identity?.IsAuthenticated != true)
        {
            return Task.CompletedTask;
        }

        // Check if user has the required permission claim
        var hasPermission = context.User.Claims
            .Any(c => c.Type == "permission" && c.Value == requirement.Permission);

        if (hasPermission)
        {
            context.Succeed(requirement);
        }

        return Task.CompletedTask;
    }
}
