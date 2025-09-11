using Clean.Architecture.Domain.Orders;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Clean.Architecture.Persistence.Configurations;

public sealed class OrderConfiguration : IEntityTypeConfiguration<Order>
{
    public void Configure(EntityTypeBuilder<Order> builder)
    {
        builder.ToTable("Orders");

        builder.HasKey(o => o.Id);

        builder.Property(o => o.Id)
            .HasConversion(
                id => id.Value,
                value => new OrderId(value))
            .IsRequired();

        builder.Property(o => o.CustomerName)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(o => o.CustomerEmail)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(o => o.ShippingAddress)
            .HasMaxLength(500)
            .IsRequired();

        builder.Property(o => o.Status)
            .HasConversion<int>()
            .IsRequired();

        builder.Property(o => o.OrderDate)
            .IsRequired();

        builder.Property(o => o.CreatedOnUtc)
            .IsRequired();

        builder.Property(o => o.ModifiedOnUtc);

        // Configure owned entity for order items
        builder.OwnsMany(o => o.Items, itemBuilder =>
        {
            itemBuilder.ToTable("OrderItems");

            itemBuilder.WithOwner().HasForeignKey("OrderId");

            itemBuilder.Property(i => i.ProductSku)
                .HasMaxLength(50)
                .IsRequired();

            itemBuilder.Property(i => i.ProductName)
                .HasMaxLength(200)
                .IsRequired();

            itemBuilder.Property(i => i.UnitPrice)
                .HasPrecision(18, 2)
                .IsRequired();

            itemBuilder.Property(i => i.Quantity)
                .IsRequired();

            itemBuilder.HasIndex(i => i.ProductSku);
        });

        // Create index on CustomerEmail for querying
        builder.HasIndex(o => o.CustomerEmail);

        // Create index on Status for filtering
        builder.HasIndex(o => o.Status);

        // Create index on OrderDate for sorting
        builder.HasIndex(o => o.OrderDate);

        // Ignore domain events as they're not persisted
        builder.Ignore("_domainEvents");
    }
}
