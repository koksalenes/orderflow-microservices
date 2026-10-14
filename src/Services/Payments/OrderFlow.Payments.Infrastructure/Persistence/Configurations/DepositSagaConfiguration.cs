using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OrderFlow.Payments.Domain.Deposits;

namespace OrderFlow.Payments.Infrastructure.Persistence.Configurations;

internal sealed class DepositSagaConfiguration : IEntityTypeConfiguration<DepositSaga>
{
    public void Configure(EntityTypeBuilder<DepositSaga> builder)
    {
        builder.ToTable("deposit_sagas");
        builder.Property(saga => saga.Id).HasColumnName("deposit_id").ValueGeneratedNever();
        builder.Property(saga => saga.OwnerId).HasMaxLength(100);
        builder.Property(saga => saga.Amount).HasPrecision(14, 2);
        builder.Property(saga => saga.Currency).HasMaxLength(3).IsFixedLength();
        builder.Property(saga => saga.CardBrand).HasMaxLength(30);
        builder.Property(saga => saga.CardLast4).HasMaxLength(4).IsFixedLength();
        builder.Property(saga => saga.AuthorizationCode).HasMaxLength(64);
        builder.Property(saga => saga.RecipientEmail).HasMaxLength(320);
        builder.Property(saga => saga.RecipientName).HasMaxLength(200);
        builder.Property(saga => saga.Locale).HasMaxLength(5);
        builder.Property(saga => saga.State).HasConversion<string>().HasMaxLength(30);
        builder.Property(saga => saga.FailureReason).HasMaxLength(100);
        builder.Property(saga => saga.Version).IsRowVersion();
        builder.Ignore(saga => saga.IsFinished);
        builder.OwnsMany(saga => saga.Steps, steps => steps.ToJson());
        builder.HasIndex(saga => new { saga.OwnerId, saga.StartedAt });
    }
}
