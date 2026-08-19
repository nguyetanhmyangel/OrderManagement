using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OrderManagement.Domain.Products;

namespace OrderManagement.Infrastructure.Configurations.Products;

public class ProductConfiguration : IEntityTypeConfiguration<Product>
{
    public void Configure(EntityTypeBuilder<Product> builder)
    {
        builder.ToTable("Products");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Sku).HasMaxLength(50).IsRequired();
        builder.Property(x => x.Name).HasMaxLength(200).IsRequired();
        builder.Property(x => x.WeightKg).HasPrecision(18, 3);
        builder.Property(x => x.IsActive);
        builder.Property(x => x.CreatedAt);
        builder.Property(x => x.UpdatedAt);

        // Owned type Money
        builder.OwnsOne(x => x.Price, price =>
        {
            price.Property(p => p.Amount).HasColumnName("PriceAmount").HasPrecision(18, 2);
            price.Property(p => p.Currency).HasColumnName("PriceCurrency").HasMaxLength(3);
        });

        // Concurrency token
        // builder.Property(x => x.Version)
        //     .IsRowVersion()
        //     .IsConcurrencyToken();
        builder.Property(x => x.Version)
            .HasColumnName("xmin")
            .HasColumnType("xid")
            .IsConcurrencyToken()
            .ValueGeneratedOnAddOrUpdate();

        // Images
        builder.HasMany(x => x.Images)
            .WithOne()
            .HasForeignKey("ProductId")
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(x => x.Images)
            .UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}
