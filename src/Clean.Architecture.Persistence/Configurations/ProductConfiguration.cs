using Clean.Architecture.Domain.Products;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Clean.Architecture.Persistence.Configurations;

public sealed class ProductConfiguration : IEntityTypeConfiguration<Product>
{
    public void Configure(EntityTypeBuilder<Product> builder)
    {
        builder.ToTable("Products");

        builder.HasKey(p => p.Id);

        builder.Property(p => p.Id)
            .HasConversion(
                id => id.Value,
                value => new ProductId(value))
            .IsRequired();

        builder.Property(p => p.Sku)
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(p => p.Name)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(p => p.Description)
            .HasMaxLength(1000);

        builder.Property(p => p.Brand)
            .HasMaxLength(100)
            .IsRequired(false);

        builder.Property(p => p.IsFeatured)
            .IsRequired();

        builder.Property(p => p.SortOrder)
            .IsRequired();

        // For now, store a simple price field until we implement proper value object mapping
        builder.Property<decimal>("Price")
            .HasColumnType("decimal(18,2)")
            .HasDefaultValue(0);

        // Ignore value objects temporarily - we'll map these properly later
        builder.Ignore(p => p.ProductPricing);
        builder.Ignore(p => p.ProductPhysicalAttributes);
        builder.Ignore(p => p.ProductSeoMetadata);
        builder.Ignore(p => p.ProductShippingInfo);
        builder.Ignore(p => p.Images);
        builder.Ignore(p => p.Tags);

        builder.Property(p => p.Category)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(p => p.IsActive)
            .IsRequired();

        builder.Property(p => p.CreatedOnUtc)
            .IsRequired();

        builder.Property(p => p.ModifiedOnUtc);

        // Create unique index on SKU
        builder.HasIndex(p => p.Sku)
            .IsUnique();

        // Create index on Category for filtering
        builder.HasIndex(p => p.Category);

        // Create index on IsActive for filtering
        builder.HasIndex(p => p.IsActive);

        // Ignore domain events as they're not persisted
        builder.Ignore("_domainEvents");
    }
}
