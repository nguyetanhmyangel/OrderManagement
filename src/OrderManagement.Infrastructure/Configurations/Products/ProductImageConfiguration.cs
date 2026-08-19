using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OrderManagement.Domain.Products;

namespace OrderManagement.Infrastructure.Configurations.Products;

public class ProductImageConfiguration : IEntityTypeConfiguration<ProductImage>
{
    public void Configure(EntityTypeBuilder<ProductImage> builder)
    {
        builder.ToTable("ProductImages");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Url).HasMaxLength(500).IsRequired();
        builder.Property(x => x.DisplayOrder);
        builder.Property(x => x.IsPrimary);
    }
}
