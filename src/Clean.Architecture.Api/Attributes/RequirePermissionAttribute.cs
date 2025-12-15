using Microsoft.AspNetCore.Authorization;

namespace Clean.Architecture.Api.Attributes;

/// <summary>
/// Attribute to require a specific permission for an endpoint.
/// </summary>
[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, AllowMultiple = true)]
public class RequirePermissionAttribute : AuthorizeAttribute
{
    /// <summary>
    /// Initializes a new instance of the <see cref="RequirePermissionAttribute"/> class.
    /// </summary>
    /// <param name="permission">The required permission.</param>
    public RequirePermissionAttribute(string permission)
    {
        Policy = permission;
    }
}
