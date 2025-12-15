namespace Clean.Architecture.Domain.Users;

/// <summary>
/// Repository interface for role operations.
/// </summary>
public interface IRoleRepository
{
    /// <summary>
    /// Gets a role by name.
    /// </summary>
    Task<Role?> GetByNameAsync(string name, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all roles.
    /// </summary>
    Task<IReadOnlyList<Role>> GetAllAsync(CancellationToken cancellationToken = default);
}
