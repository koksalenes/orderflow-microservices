using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OrderFlow.Orders.Domain.Orders;

namespace OrderFlow.Orders.Infrastructure.Persistence.Configurations;

internal sealed class OrderConfiguration : IEntityTypeConfiguration<Order>
{
    public void Configure(EntityTypeBuilder<Order> builder)
    {
        builder.ToTable("orders");
        builder.Property(order => order.Id).ValueGeneratedNever();
        builder.Property(order => order.Number).HasMaxLength(20);
        builder.HasIndex(order => order.Number).IsUnique();
        builder.Property(order => order.CustomerId).HasMaxLength(100);
        builder.Property(order => order.CustomerEmail).HasMaxLength(320);
        builder.Property(order => order.CustomerName).HasMaxLength(200);
        builder.Property(order => order.Locale).HasMaxLength(5);
        builder.Property(order => order.Currency).HasMaxLength(3).IsFixedLength();
        builder.Property(order => order.Subtotal).HasPrecision(14, 2);
        builder.Property(order => order.ShippingFee).HasPrecision(14, 2);
        builder.Property(order => order.Total).HasPrecision(14, 2);
        builder.Property(order => order.PaymentMethod).HasConversion<string>().HasMaxLength(10);
        builder.Property(order => order.CardBrand).HasMaxLength(30);
        builder.Property(order => order.CardLast4).HasMaxLength(4).IsFixedLength();
        builder.Property(order => order.Scenario).HasMaxLength(30);
        builder.Property(order => order.Status).HasConversion<string>().HasMaxLength(20);
        builder.Property(order => order.CancellationReason).HasMaxLength(100);
        builder.Property(order => order.PaymentReference).HasMaxLength(200);
        builder.Property(order => order.TrackingNumber).HasMaxLength(30);
        builder.Property(order => order.InvoiceNumber).HasMaxLength(30);
        builder.Property(order => order.Version).IsRowVersion();
        builder.OwnsMany(order => order.Lines, lines => lines.ToJson());
        builder.OwnsOne(order => order.ShippingAddress, address => address.ToJson());
        builder.OwnsMany(order => order.Steps, steps => steps.ToJson());
        builder.HasIndex(order => new { order.CustomerId, order.PlacedAt });
        builder.HasIndex(order => order.NextActionAt).HasFilter("next_action_at IS NOT NULL");
    }
}
