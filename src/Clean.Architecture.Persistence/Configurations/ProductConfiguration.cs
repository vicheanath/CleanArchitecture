using Clean.Architecture.Domain.Products;
using Clean.Architecture.Domain.Products.ValueObjects;
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

        // Map Pricing value object
        builder.OwnsOne(p => p.ProductPricing, pricingBuilder =>
        {
            pricingBuilder.Property(pr => pr.RegularPrice)
                .HasColumnName("RegularPrice")
                .HasColumnType("decimal(18,2)")
                .IsRequired();

            pricingBuilder.Property(pr => pr.SalePrice)
                .HasColumnName("SalePrice")
                .HasColumnType("decimal(18,2)");

            pricingBuilder.Property(pr => pr.SaleStartDate)
                .HasColumnName("SaleStartDate");

            pricingBuilder.Property(pr => pr.SaleEndDate)
                .HasColumnName("SaleEndDate");
        });

        // Map PhysicalAttributes value object
        builder.OwnsOne(p => p.ProductPhysicalAttributes, physicalBuilder =>
        {
            physicalBuilder.Property(pa => pa.Weight)
                .HasColumnName("Weight")
                .HasColumnType("decimal(18,2)");

            physicalBuilder.Property(pa => pa.Dimensions)
                .HasColumnName("Dimensions")
                .HasMaxLength(100);

            physicalBuilder.Property(pa => pa.Color)
                .HasColumnName("Color")
                .HasMaxLength(50);

            physicalBuilder.Property(pa => pa.Size)
                .HasColumnName("Size")
                .HasMaxLength(50);
        });

        // Map SeoMetadata value object
        builder.OwnsOne(p => p.ProductSeoMetadata, seoBuilder =>
        {
            seoBuilder.Property(sm => sm.MetaTitle)
                .HasColumnName("MetaTitle")
                .HasMaxLength(200);

            seoBuilder.Property(sm => sm.MetaDescription)
                .HasColumnName("MetaDescription")
                .HasMaxLength(500);
        });

        // Map ShippingInfo value object
        builder.OwnsOne(p => p.ProductShippingInfo, shippingBuilder =>
        {
            shippingBuilder.Property(si => si.RequiresShipping)
                .HasColumnName("RequiresShipping")
                .IsRequired();

            shippingBuilder.Property(si => si.ShippingWeight)
                .HasColumnName("ShippingWeight")
                .HasColumnType("decimal(18,2)");
        });

        // Map ProductImages value object as JSON (for in-memory DB, we'll store as comma-separated)
        // For production, consider using a proper JSON column type
        builder.Property(p => p.Images)
            .HasConversion(
                images => string.Join("|||", images.ImageUrls),
                value => ProductImages.Create(
                    string.IsNullOrEmpty(value) ? Array.Empty<string>() : value.Split("|||", StringSplitOptions.RemoveEmptyEntries)))
            .HasColumnName("Images")
            .HasMaxLength(2000);

        // Map ProductTags value object as JSON (for in-memory DB, we'll store as comma-separated)
        builder.Property(p => p.Tags)
            .HasConversion(
                tags => string.Join("|||", tags.Tags),
                value => ProductTags.Create(
                    string.IsNullOrEmpty(value) ? Array.Empty<string>() : value.Split("|||", StringSplitOptions.RemoveEmptyEntries)))
            .HasColumnName("Tags")
            .HasMaxLength(1000);

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
