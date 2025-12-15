using Clean.Architecture.Application.Auth.DTOs;
using Shared.Messaging;

namespace Clean.Architecture.Application.Auth.Login;

/// <summary>
/// Command to login a user.
/// </summary>
/// <param name="Email">The user email.</param>
/// <param name="Password">The user password.</param>
public record LoginCommand(string Email, string Password) : ICommand<AuthResponse>;
