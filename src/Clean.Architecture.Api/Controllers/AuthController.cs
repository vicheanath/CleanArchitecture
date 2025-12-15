using Clean.Architecture.Application.Auth.DTOs;
using Clean.Architecture.Application.Auth.Login;
using Clean.Architecture.Application.Auth.Register;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shared.Messaging;
using Shared.Results;

namespace Clean.Architecture.Api.Controllers;

/// <summary>
/// Controller for authentication operations.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly ICommandHandler<LoginCommand, AuthResponse> _loginHandler;
    private readonly ICommandHandler<RegisterCommand, AuthResponse> _registerHandler;

    /// <summary>
    /// Initializes a new instance of the <see cref="AuthController"/> class.
    /// </summary>
    public AuthController(
        ICommandHandler<LoginCommand, AuthResponse> loginHandler,
        ICommandHandler<RegisterCommand, AuthResponse> registerHandler)
    {
        _loginHandler = loginHandler;
        _registerHandler = registerHandler;
    }

    /// <summary>
    /// Logs in a user and returns a JWT token.
    /// </summary>
    /// <param name="request">The login request.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The authentication response with JWT token.</returns>
    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<ActionResult<Result<AuthResponse>>> Login(
        [FromBody] LoginRequest request,
        CancellationToken cancellationToken)
    {
        var command = new LoginCommand(request.Email, request.Password);
        var result = await _loginHandler.Handle(command, cancellationToken);
        return result;
    }

    /// <summary>
    /// Registers a new user and returns a JWT token.
    /// </summary>
    /// <param name="request">The registration request.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The authentication response with JWT token.</returns>
    [HttpPost("register")]
    [AllowAnonymous]
    public async Task<ActionResult<Result<AuthResponse>>> Register(
        [FromBody] RegisterRequest request,
        CancellationToken cancellationToken)
    {
        var command = new RegisterCommand(
            request.Email,
            request.Password,
            request.FirstName,
            request.LastName);

        var result = await _registerHandler.Handle(command, cancellationToken);

        if (result.IsSuccess)
        {
            return CreatedAtAction(nameof(Login), new { email = request.Email }, result);
        }

        return result;
    }
}

/// <summary>
/// Request model for user login.
/// </summary>
/// <param name="Email">The user email.</param>
/// <param name="Password">The user password.</param>
public record LoginRequest(string Email, string Password);

/// <summary>
/// Request model for user registration.
/// </summary>
/// <param name="Email">The user email.</param>
/// <param name="Password">The user password.</param>
/// <param name="FirstName">The user's first name.</param>
/// <param name="LastName">The user's last name.</param>
public record RegisterRequest(
    string Email,
    string Password,
    string FirstName,
    string LastName);
