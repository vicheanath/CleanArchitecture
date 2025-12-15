using Clean.Architecture.Domain.Users;

namespace Clean.Architecture.Application.Auth.Common;

/// <summary>
/// Service for generating JWT tokens.
/// </summary>
public interface IJwtTokenService
{
    /// <summary>
    /// Generates a JWT token for a user.
    /// </summary>
    /// <param name="user">The user.</param>
    /// <returns>The JWT token string.</returns>
    string GenerateToken(User user);
}
