using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OrderFlow.Inventory.Domain.Stock;

namespace OrderFlow.Inventory.Infrastructure.Persistence.Configurations;

internal sealed class StockItemConfiguration : IEntityTypeConfiguration<StockItem>
{
    public void Configure(EntityTypeBuilder<StockItem> builder)
    {
        builder.ToTable(
            "stock_items",
            table =>
            {
                table.HasCheckConstraint("ck_stock_items_on_hand", "on_hand >= 0");
                table.HasCheckConstraint(
                    "ck_stock_items_reserved",
                    "reserved >= 0 AND reserved <= on_hand"
                );
            }
        );

        builder.Property(item => item.Id).HasColumnName("product_id").ValueGeneratedNever();
        builder.Property(item => item.Reserved);
        builder.Ignore(item => item.Available);

        builder.Property(item => item.Version).IsRowVersion();
    }
}
