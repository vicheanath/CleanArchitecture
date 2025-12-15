using Clean.Architecture.Application.Auth.DTOs;
using Shared.Messaging;

namespace Clean.Architecture.Application.Auth.Register;

/// <summary>
/// Command to register a new user.
/// </summary>
/// <param name="Email">The user email.</param>
/// <param name="Password">The user password.</param>
/// <param name="FirstName">The user's first name.</param>
/// <param name="LastName">The user's last name.</param>
public record RegisterCommand(
    string Email,
    string Password,
    string FirstName,
    string LastName) : ICommand<AuthResponse>;
