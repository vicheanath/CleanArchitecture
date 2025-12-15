using Clean.Architecture.Domain.Users;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Clean.Architecture.Persistence.Users;

public sealed class RoleConfiguration : IEntityTypeConfiguration<Role>
{
    public void Configure(EntityTypeBuilder<Role> builder)
    {
        builder.ToTable("Roles");

        builder.HasKey(r => r.Id);

        builder.Property(r => r.Id)
            .HasConversion(
                id => id.Value,
                value => new RoleId(value))
            .IsRequired();

        builder.Property(r => r.Name)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(r => r.Description)
            .HasMaxLength(500);

        // Store permissions as a comma-separated string
        builder.Property(r => r.Permissions)
            .HasConversion(
                permissions => string.Join("|||", permissions),
                value => string.IsNullOrEmpty(value)
                    ? new List<string>()
                    : value.Split("|||", StringSplitOptions.RemoveEmptyEntries).ToList())
            .HasColumnName("Permissions")
            .HasMaxLength(2000);

        // Create unique index on Name
        builder.HasIndex(r => r.Name)
            .IsUnique();

        // Ignore domain events
        builder.Ignore("_domainEvents");
    }
}
