using Clean.Architecture.Application.Auth.Common;
using Clean.Architecture.Application.Auth.DTOs;
using Clean.Architecture.Application.Common.Interfaces;
using Clean.Architecture.Domain.Users;
using Shared.Errors;
using Shared.Messaging;
using Shared.Results;

namespace Clean.Architecture.Application.Auth.Register;

public sealed class RegisterCommandHandler : ICommandHandler<RegisterCommand, AuthResponse>
{
    private readonly IUserRepository _userRepository;
    private readonly IRoleRepository _roleRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtTokenService _jwtTokenService;
    private readonly IUnitOfWork _unitOfWork;

    public RegisterCommandHandler(
        IUserRepository userRepository,
        IRoleRepository roleRepository,
        IPasswordHasher passwordHasher,
        IJwtTokenService jwtTokenService,
        IUnitOfWork unitOfWork)
    {
        _userRepository = userRepository;
        _roleRepository = roleRepository;
        _passwordHasher = passwordHasher;
        _jwtTokenService = jwtTokenService;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<AuthResponse>> Handle(RegisterCommand command, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(command.Email))
            return Result.Failure<AuthResponse>(UserErrors.InvalidEmail);

        if (string.IsNullOrWhiteSpace(command.Password))
            return Result.Failure<AuthResponse>(UserErrors.InvalidPassword);

        if (command.Password.Length < 6)
            return Result.Failure<AuthResponse>(new Error("User.WeakPassword", "Password must be at least 6 characters long"));

        if (string.IsNullOrWhiteSpace(command.FirstName))
            return Result.Failure<AuthResponse>(new Error("User.InvalidFirstName", "First name cannot be empty"));

        if (string.IsNullOrWhiteSpace(command.LastName))
            return Result.Failure<AuthResponse>(new Error("User.InvalidLastName", "Last name cannot be empty"));

        // Check if user already exists
        var existingUser = await _userRepository.GetByEmailAsync(command.Email, cancellationToken);
        if (existingUser != null)
            return Result.Failure<AuthResponse>(UserErrors.DuplicateEmail);

        // Hash password
        var passwordHash = _passwordHasher.HashPassword(command.Password);

        // Create user
        var user = User.Create(command.Email, passwordHash, command.FirstName, command.LastName);

        // Assign "User" role by default
        var userRole = await _roleRepository.GetByNameAsync("User", cancellationToken);
        if (userRole != null)
        {
            user.AddRole(userRole);
        }

        await _userRepository.AddAsync(user, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Reload user with roles for token generation
        var savedUser = await _userRepository.GetByIdAsync(user.Id, cancellationToken);
        if (savedUser == null)
        {
            return Result.Failure<AuthResponse>(new Error("User.SaveFailed", "Failed to save user"));
        }

        // Generate token
        var token = _jwtTokenService.GenerateToken(savedUser);

        var response = new AuthResponse(
            token,
            savedUser.Id.Value,
            savedUser.Email,
            savedUser.FirstName,
            savedUser.LastName,
            savedUser.Roles.Select(r => r.Name).ToList(),
            savedUser.Permissions.ToList());

        return Result.Success(response);
    }
}
