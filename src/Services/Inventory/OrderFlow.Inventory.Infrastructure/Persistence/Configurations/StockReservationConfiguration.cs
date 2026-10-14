using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OrderFlow.Inventory.Domain.Reservations;

namespace OrderFlow.Inventory.Infrastructure.Persistence.Configurations;

internal sealed class StockReservationConfiguration : IEntityTypeConfiguration<StockReservation>
{
    public void Configure(EntityTypeBuilder<StockReservation> builder)
    {
        builder.ToTable("stock_reservations");
        builder
            .Property(reservation => reservation.Id)
            .HasColumnName("order_id")
            .ValueGeneratedNever();
        builder
            .Property(reservation => reservation.Status)
            .HasConversion<string>()
            .HasMaxLength(20);
        builder.Property(reservation => reservation.Reason).HasMaxLength(100);
        builder.Property(reservation => reservation.Version).IsRowVersion();
        builder.OwnsMany(reservation => reservation.Lines, lines => lines.ToJson());
    }
}
