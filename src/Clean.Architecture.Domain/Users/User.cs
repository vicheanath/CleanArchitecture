using Shared.Primitives;

namespace Clean.Architecture.Domain.Users;

/// <summary>
/// Represents a user entity.
/// </summary>
public sealed class User : Entity<UserId>, IAuditable
{
    private readonly List<Role> _roles = new();

    private User(UserId id, string email, string passwordHash, string firstName, string lastName)
        : base(id)
    {
        Email = email;
        PasswordHash = passwordHash;
        FirstName = firstName;
        LastName = lastName;
        IsActive = true;
        CreatedOnUtc = DateTime.UtcNow;
        ModifiedOnUtc = null;
    }

    /// <summary>
    /// Private constructor for EF Core.
    /// </summary>
    private User() : base(UserId.New())
    {
        Email = string.Empty;
        PasswordHash = string.Empty;
        FirstName = string.Empty;
        LastName = string.Empty;
        IsActive = false;
        CreatedOnUtc = DateTime.UtcNow;
        ModifiedOnUtc = null;
    }

    /// <summary>
    /// Gets the user email address.
    /// </summary>
    public string Email { get; private set; } = string.Empty;

    /// <summary>
    /// Gets the password hash.
    /// </summary>
    public string PasswordHash { get; private set; } = string.Empty;

    /// <summary>
    /// Gets the user's first name.
    /// </summary>
    public string FirstName { get; private set; } = string.Empty;

    /// <summary>
    /// Gets the user's last name.
    /// </summary>
    public string LastName { get; private set; } = string.Empty;

    /// <summary>
    /// Gets the user's full name.
    /// </summary>
    public string FullName => $"{FirstName} {LastName}".Trim();

    /// <summary>
    /// Gets a value indicating whether the user is active.
    /// </summary>
    public bool IsActive { get; private set; }

    /// <summary>
    /// Gets the roles assigned to this user.
    /// </summary>
    public IReadOnlyList<Role> Roles => _roles.AsReadOnly();

    /// <summary>
    /// Gets all permissions from all roles assigned to this user.
    /// </summary>
    public IReadOnlyList<string> Permissions => _roles
        .SelectMany(r => r.Permissions)
        .Distinct()
        .ToList()
        .AsReadOnly();

    /// <inheritdoc />
    public DateTime CreatedOnUtc { get; private set; }

    /// <inheritdoc />
    public DateTime? ModifiedOnUtc { get; private set; }

    /// <summary>
    /// Creates a new user.
    /// </summary>
    /// <param name="email">The user email.</param>
    /// <param name="passwordHash">The hashed password.</param>
    /// <param name="firstName">The user's first name.</param>
    /// <param name="lastName">The user's last name.</param>
    /// <returns>The newly created user.</returns>
    public static User Create(string email, string passwordHash, string firstName, string lastName)
    {
        if (string.IsNullOrWhiteSpace(email))
            throw new ArgumentException("Email cannot be empty", nameof(email));

        if (!IsValidEmail(email))
            throw new ArgumentException("Invalid email format", nameof(email));

        if (string.IsNullOrWhiteSpace(passwordHash))
            throw new ArgumentException("Password hash cannot be empty", nameof(passwordHash));

        if (string.IsNullOrWhiteSpace(firstName))
            throw new ArgumentException("First name cannot be empty", nameof(firstName));

        if (string.IsNullOrWhiteSpace(lastName))
            throw new ArgumentException("Last name cannot be empty", nameof(lastName));

        return new User(UserId.New(), email.ToLowerInvariant(), passwordHash, firstName, lastName);
    }

    /// <summary>
    /// Adds a role to the user.
    /// </summary>
    /// <param name="role">The role to add.</param>
    public void AddRole(Role role)
    {
        if (role == null)
            throw new ArgumentNullException(nameof(role));

        if (!_roles.Contains(role))
        {
            _roles.Add(role);
            ModifiedOnUtc = DateTime.UtcNow;
        }
    }

    /// <summary>
    /// Removes a role from the user.
    /// </summary>
    /// <param name="role">The role to remove.</param>
    public void RemoveRole(Role role)
    {
        if (role == null)
            throw new ArgumentNullException(nameof(role));

        if (_roles.Remove(role))
        {
            ModifiedOnUtc = DateTime.UtcNow;
        }
    }

    /// <summary>
    /// Checks if the user has a specific permission.
    /// </summary>
    /// <param name="permission">The permission to check.</param>
    /// <returns>True if the user has the permission, otherwise false.</returns>
    public bool HasPermission(string permission)
    {
        return _roles.Any(r => r.HasPermission(permission));
    }

    /// <summary>
    /// Updates the user's password.
    /// </summary>
    /// <param name="newPasswordHash">The new password hash.</param>
    public void UpdatePassword(string newPasswordHash)
    {
        if (string.IsNullOrWhiteSpace(newPasswordHash))
            throw new ArgumentException("Password hash cannot be empty", nameof(newPasswordHash));

        PasswordHash = newPasswordHash;
        ModifiedOnUtc = DateTime.UtcNow;
    }

    /// <summary>
    /// Updates the user's profile information.
    /// </summary>
    /// <param name="firstName">The new first name.</param>
    /// <param name="lastName">The new last name.</param>
    public void UpdateProfile(string firstName, string lastName)
    {
        if (string.IsNullOrWhiteSpace(firstName))
            throw new ArgumentException("First name cannot be empty", nameof(firstName));

        if (string.IsNullOrWhiteSpace(lastName))
            throw new ArgumentException("Last name cannot be empty", nameof(lastName));

        FirstName = firstName;
        LastName = lastName;
        ModifiedOnUtc = DateTime.UtcNow;
    }

    /// <summary>
    /// Activates the user account.
    /// </summary>
    public void Activate()
    {
        IsActive = true;
        ModifiedOnUtc = DateTime.UtcNow;
    }

    /// <summary>
    /// Deactivates the user account.
    /// </summary>
    public void Deactivate()
    {
        IsActive = false;
        ModifiedOnUtc = DateTime.UtcNow;
    }

    private static bool IsValidEmail(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
            return false;

        try
        {
            var addr = new System.Net.Mail.MailAddress(email);
            return addr.Address == email;
        }
        catch
        {
            return false;
        }
    }
}
