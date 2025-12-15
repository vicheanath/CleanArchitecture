using Microsoft.AspNetCore.Authorization;

namespace Clean.Architecture.Application.Common.Authorization;

/// <summary>
/// Authorization requirement for permission-based access control.
/// </summary>
public class PermissionRequirement : IAuthorizationRequirement
{
    /// <summary>
    /// Initializes a new instance of the <see cref="PermissionRequirement"/> class.
    /// </summary>
    /// <param name="permission">The required permission.</param>
    public PermissionRequirement(string permission)
    {
        Permission = permission ?? throw new ArgumentNullException(nameof(permission));
    }

    /// <summary>
    /// Gets the required permission.
    /// </summary>
    public string Permission { get; }
}
