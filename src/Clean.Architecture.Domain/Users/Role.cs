using Shared.Primitives;

namespace Clean.Architecture.Domain.Users;

/// <summary>
/// Represents a role entity that contains permissions.
/// </summary>
public sealed class Role : Entity<RoleId>
{
    private readonly List<string> _permissions = new();

    private Role(RoleId id, string name, string description)
        : base(id)
    {
        Name = name;
        Description = description;
    }

    /// <summary>
    /// Private constructor for EF Core.
    /// </summary>
    private Role() : base(RoleId.New())
    {
        Name = string.Empty;
        Description = string.Empty;
    }

    /// <summary>
    /// Gets the role name.
    /// </summary>
    public string Name { get; private set; } = string.Empty;

    /// <summary>
    /// Gets the role description.
    /// </summary>
    public string Description { get; private set; } = string.Empty;

    /// <summary>
    /// Gets the permissions assigned to this role.
    /// </summary>
    public IReadOnlyList<string> Permissions => _permissions.AsReadOnly();

    /// <summary>
    /// Creates a new role.
    /// </summary>
    /// <param name="name">The role name.</param>
    /// <param name="description">The role description.</param>
    /// <returns>The newly created role.</returns>
    public static Role Create(string name, string description)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Role name cannot be empty", nameof(name));

        if (string.IsNullOrWhiteSpace(description))
            throw new ArgumentException("Role description cannot be empty", nameof(description));

        return new Role(RoleId.New(), name, description);
    }

    /// <summary>
    /// Adds a permission to the role.
    /// </summary>
    /// <param name="permission">The permission to add.</param>
    public void AddPermission(string permission)
    {
        if (string.IsNullOrWhiteSpace(permission))
            throw new ArgumentException("Permission cannot be empty", nameof(permission));

        if (!Permission.IsValid(permission))
            throw new ArgumentException($"Invalid permission: {permission}", nameof(permission));

        if (!_permissions.Contains(permission))
        {
            _permissions.Add(permission);
        }
    }

    /// <summary>
    /// Removes a permission from the role.
    /// </summary>
    /// <param name="permission">The permission to remove.</param>
    public void RemovePermission(string permission)
    {
        if (string.IsNullOrWhiteSpace(permission))
            throw new ArgumentException("Permission cannot be empty", nameof(permission));

        _permissions.Remove(permission);
    }

    /// <summary>
    /// Checks if the role has a specific permission.
    /// </summary>
    /// <param name="permission">The permission to check.</param>
    /// <returns>True if the role has the permission, otherwise false.</returns>
    public bool HasPermission(string permission)
    {
        return _permissions.Contains(permission);
    }
}
