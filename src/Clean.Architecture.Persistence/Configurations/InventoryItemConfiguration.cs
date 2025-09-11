using Clean.Architecture.Domain.Inventory;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Clean.Architecture.Persistence.Configurations;

public sealed class InventoryItemConfiguration : IEntityTypeConfiguration<InventoryItem>
{
    public void Configure(EntityTypeBuilder<InventoryItem> builder)
    {
        builder.ToTable("InventoryItems");

        builder.HasKey(i => i.Id);

        builder.Property(i => i.Id)
            .HasConversion(
                id => id.Value,
                value => new InventoryItemId(value))
            .IsRequired();

        builder.Property(i => i.ProductSku)
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(i => i.Quantity)
            .IsRequired();

        builder.Property(i => i.MinimumStockLevel)
            .IsRequired();

        builder.Property(i => i.CreatedOnUtc)
            .IsRequired();

        builder.Property(i => i.ModifiedOnUtc);

        // Configure owned entity for reservations
        builder.OwnsMany(i => i.Reservations, reservationBuilder =>
        {
            reservationBuilder.ToTable("InventoryReservations");

            reservationBuilder.WithOwner().HasForeignKey("InventoryItemId");

            reservationBuilder.Property(r => r.ReservationId)
                .HasMaxLength(100)
                .IsRequired();

            reservationBuilder.Property(r => r.Quantity)
                .IsRequired();

            reservationBuilder.Property(r => r.ReservedAt)
                .IsRequired();

            reservationBuilder.Property(r => r.ExpiresAt);

            reservationBuilder.HasIndex(r => r.ReservationId);
            reservationBuilder.HasIndex(r => r.ExpiresAt);
        });

        // Create unique index on ProductSku
        builder.HasIndex(i => i.ProductSku)
            .IsUnique();

        // Create index on MinimumStockLevel for queries
        builder.HasIndex(i => i.MinimumStockLevel);

        // Ignore domain events as they're not persisted
        builder.Ignore("_domainEvents");
    }
}
