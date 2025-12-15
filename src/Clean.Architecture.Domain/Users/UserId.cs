using Shared.Primitives;

namespace Clean.Architecture.Domain.Users;

/// <summary>
/// Represents the user identifier.
/// </summary>
/// <param name="Value">The identifier value.</param>
public sealed record UserId(Guid Value) : IEntityId
{
    /// <summary>
    /// Parameterless constructor for serialization
    /// </summary>
    public UserId() : this(Guid.Empty) { }

    public static UserId New() => new(Guid.NewGuid());
    public static UserId Create(Guid value) => new(value);
}
