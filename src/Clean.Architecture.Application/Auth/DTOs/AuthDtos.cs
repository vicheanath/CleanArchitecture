namespace Clean.Architecture.Application.Auth.DTOs;

/// <summary>
/// Response DTO for login and registration.
/// </summary>
/// <param name="Token">The JWT token.</param>
/// <param name="UserId">The user ID.</param>
/// <param name="Email">The user email.</param>
/// <param name="FirstName">The user's first name.</param>
/// <param name="LastName">The user's last name.</param>
/// <param name="Roles">The user's roles.</param>
/// <param name="Permissions">The user's permissions.</param>
public record AuthResponse(
    string Token,
    Guid UserId,
    string Email,
    string FirstName,
    string LastName,
    IReadOnlyList<string> Roles,
    IReadOnlyList<string> Permissions);
