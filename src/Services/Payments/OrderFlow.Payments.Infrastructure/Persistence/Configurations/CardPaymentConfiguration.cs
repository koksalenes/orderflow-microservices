using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OrderFlow.Payments.Domain.CardPayments;

namespace OrderFlow.Payments.Infrastructure.Persistence.Configurations;

internal sealed class CardPaymentConfiguration : IEntityTypeConfiguration<CardPayment>
{
    public void Configure(EntityTypeBuilder<CardPayment> builder)
    {
        builder.ToTable("card_payments");
        builder.Property(payment => payment.Id).ValueGeneratedNever();
        builder.Property(payment => payment.OwnerId).HasMaxLength(100);
        builder.Property(payment => payment.Amount).HasPrecision(14, 2);
        builder.Property(payment => payment.Currency).HasMaxLength(3).IsFixedLength();
        builder.Property(payment => payment.CardBrand).HasMaxLength(30);
        builder.Property(payment => payment.CardLast4).HasMaxLength(4).IsFixedLength();
        builder.Property(payment => payment.AuthorizationCode).HasMaxLength(64);
        builder.Property(payment => payment.Status).HasConversion<string>().HasMaxLength(20);
        builder.Property(payment => payment.FailureReason).HasMaxLength(100);
        builder.Property(payment => payment.Version).IsRowVersion();
        builder.HasIndex(payment => payment.OrderId);
    }
}
