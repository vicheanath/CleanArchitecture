using Clean.Architecture.Application.Auth.Common;
using Clean.Architecture.Application.Auth.DTOs;
using Clean.Architecture.Domain.Users;
using Shared.Errors;
using Shared.Messaging;
using Shared.Results;

namespace Clean.Architecture.Application.Auth.Login;

public sealed class LoginCommandHandler : ICommandHandler<LoginCommand, AuthResponse>
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtTokenService _jwtTokenService;

    public LoginCommandHandler(
        IUserRepository userRepository,
        IPasswordHasher passwordHasher,
        IJwtTokenService jwtTokenService)
    {
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
        _jwtTokenService = jwtTokenService;
    }

    public async Task<Result<AuthResponse>> Handle(LoginCommand command, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(command.Email))
            return Result.Failure<AuthResponse>(UserErrors.InvalidEmail);

        if (string.IsNullOrWhiteSpace(command.Password))
            return Result.Failure<AuthResponse>(UserErrors.InvalidPassword);

        var user = await _userRepository.GetByEmailAsync(command.Email, cancellationToken);
        if (user == null)
            return Result.Failure<AuthResponse>(UserErrors.InvalidCredentials);

        if (!user.IsActive)
            return Result.Failure<AuthResponse>(new Error("User.Inactive", "User account is inactive"));

        if (!_passwordHasher.VerifyPassword(command.Password, user.PasswordHash))
            return Result.Failure<AuthResponse>(UserErrors.InvalidCredentials);

        var token = _jwtTokenService.GenerateToken(user);

        var response = new AuthResponse(
            token,
            user.Id.Value,
            user.Email,
            user.FirstName,
            user.LastName,
            user.Roles.Select(r => r.Name).ToList(),
            user.Permissions.ToList());

        return Result.Success(response);
    }
}
