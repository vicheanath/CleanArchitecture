using Shared.Errors;

namespace Clean.Architecture.Domain.Users;

public static class UserErrors
{
    public static readonly Error NotFound = new("User.NotFound", "The user with the specified ID was not found.");

    public static readonly Error InvalidEmail = new("User.InvalidEmail", "Email cannot be empty or invalid.");

    public static readonly Error InvalidPassword = new("User.InvalidPassword", "Password cannot be empty or too weak.");

    public static readonly Error DuplicateEmail = new("User.DuplicateEmail", "A user with this email already exists.");

    public static readonly Error InvalidCredentials = new("User.InvalidCredentials", "Invalid email or password.");

    public static Error NotFoundWithId(Guid userId) =>
        new("User.NotFound", $"The user with ID '{userId}' was not found.");

    public static Error NotFoundWithEmail(string email) =>
        new("User.NotFound", $"The user with email '{email}' was not found.");
}
