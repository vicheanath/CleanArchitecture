using Shared.Primitives;

namespace Clean.Architecture.Domain.Users;

/// <summary>
/// Represents the role identifier.
/// </summary>
/// <param name="Value">The identifier value.</param>
public sealed record RoleId(Guid Value) : IEntityId
{
    /// <summary>
    /// Parameterless constructor for serialization
    /// </summary>
    public RoleId() : this(Guid.Empty) { }

    public static RoleId New() => new(Guid.NewGuid());
    public static RoleId Create(Guid value) => new(value);
}
