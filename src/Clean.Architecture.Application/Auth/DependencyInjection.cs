using Clean.Architecture.Application.Auth.Common;
using Clean.Architecture.Application.Auth.Login;
using Clean.Architecture.Application.Auth.Register;
using Clean.Architecture.Application.Auth.DTOs;
using Microsoft.Extensions.DependencyInjection;
using Shared.Messaging;

namespace Clean.Architecture.Application.Auth;

public static class DependencyInjection
{
    public static IServiceCollection AddAuth(this IServiceCollection services)
    {
        services.AddScoped<IPasswordHasher, PasswordHasher>();
        services.AddScoped<IJwtTokenService, JwtTokenService>();

        // Register Command Handlers
        services.AddScoped<ICommandHandler<LoginCommand, AuthResponse>, LoginCommandHandler>();
        services.AddScoped<ICommandHandler<RegisterCommand, AuthResponse>, RegisterCommandHandler>();

        return services;
    }
}
