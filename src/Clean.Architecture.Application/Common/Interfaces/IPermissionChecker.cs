namespace Clean.Architecture.Application.Common.Interfaces;

/// <summary>
/// Service for checking user permissions.
/// </summary>
public interface IPermissionChecker
{
    /// <summary>
    /// Checks if the current user has a specific permission.
    /// </summary>
    /// <param name="permission">The permission to check.</param>
    /// <returns>True if the user has the permission, otherwise false.</returns>
    bool HasPermission(string permission);

    /// <summary>
    /// Gets all permissions for the current user.
    /// </summary>
    /// <returns>The list of permissions.</returns>
    IReadOnlyList<string> GetPermissions();
}
