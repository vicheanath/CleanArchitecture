using System.Security.Claims;
using Clean.Architecture.Application.Common.Interfaces;
using Microsoft.AspNetCore.Http;

namespace Clean.Architecture.Application.Common.Services;

public sealed class PermissionChecker : IPermissionChecker
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public PermissionChecker(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public bool HasPermission(string permission)
    {
        var user = _httpContextAccessor.HttpContext?.User;
        if (user == null || user.Identity?.IsAuthenticated != true)
            return false;

        // Check if user has the permission claim
        var permissions = user.Claims
            .Where(c => c.Type == "permission")
            .Select(c => c.Value)
            .ToList();

        return permissions.Contains(permission);
    }

    public IReadOnlyList<string> GetPermissions()
    {
        var user = _httpContextAccessor.HttpContext?.User;
        if (user == null || user.Identity?.IsAuthenticated != true)
            return Array.Empty<string>();

        return user.Claims
            .Where(c => c.Type == "permission")
            .Select(c => c.Value)
            .ToList()
            .AsReadOnly();
    }
}
