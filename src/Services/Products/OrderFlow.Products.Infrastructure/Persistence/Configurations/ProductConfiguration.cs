using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OrderFlow.Products.Domain.Products;

namespace OrderFlow.Products.Infrastructure.Persistence.Configurations;

internal sealed class ProductConfiguration : IEntityTypeConfiguration<Product>
{
    public void Configure(EntityTypeBuilder<Product> builder)
    {
        builder.Property(product => product.Id).ValueGeneratedNever();
        builder.Property(product => product.Name).HasMaxLength(200);
        builder.Property(product => product.Description).HasMaxLength(4000);
        builder.Property(product => product.Category).HasMaxLength(100);
        builder.Property(product => product.Price).HasPrecision(12, 2);
        builder.Property(product => product.Currency).HasMaxLength(3).IsFixedLength();

        builder.PrimitiveCollection(product => product.Images);

        builder.OwnsMany(product => product.Specifications, owned => owned.ToJson());
    }
}
