using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OrderFlow.Payments.Infrastructure.EventStore;

namespace OrderFlow.Payments.Infrastructure.Persistence.Configurations;

internal sealed class StoredEventConfiguration : IEntityTypeConfiguration<StoredEvent>
{
    public void Configure(EntityTypeBuilder<StoredEvent> builder)
    {
        builder.ToTable("events");
        builder.HasKey(stored => stored.GlobalPosition);
        builder.Property(stored => stored.GlobalPosition).UseIdentityAlwaysColumn();
        builder.Property(stored => stored.StreamId).HasMaxLength(200);
        builder.Property(stored => stored.EventType).HasMaxLength(200);
        builder.Property(stored => stored.Data).HasColumnType("jsonb");
        builder.Property(stored => stored.Metadata).HasColumnType("jsonb");
        builder.Property(stored => stored.RecordedAt).HasDefaultValueSql("now()");

        builder
            .HasIndex(stored => new { stored.StreamId, stored.Version })
            .IsUnique()
            .HasDatabaseName(EventStoreConstraints.StreamVersion);
        builder
            .HasIndex(stored => stored.EventId)
            .IsUnique()
            .HasDatabaseName(EventStoreConstraints.EventId);
    }
}

internal sealed class StoredSnapshotConfiguration : IEntityTypeConfiguration<StoredSnapshot>
{
    public void Configure(EntityTypeBuilder<StoredSnapshot> builder)
    {
        builder.ToTable("snapshots");
        builder.HasKey(snapshot => snapshot.StreamId);
        builder.Property(snapshot => snapshot.StreamId).HasMaxLength(200);
        builder.Property(snapshot => snapshot.State).HasColumnType("jsonb");
    }
}

internal sealed class WalletBalanceRowConfiguration : IEntityTypeConfiguration<WalletBalanceRow>
{
    public void Configure(EntityTypeBuilder<WalletBalanceRow> builder)
    {
        builder.ToTable("wallet_balances");
        builder.HasKey(row => row.OwnerId);
        builder.Property(row => row.OwnerId).HasMaxLength(100);
        builder.Property(row => row.Balance).HasPrecision(14, 2);
        builder.Property(row => row.Currency).HasMaxLength(3).IsFixedLength();
    }
}

internal sealed class WalletTransactionRowConfiguration
    : IEntityTypeConfiguration<WalletTransactionRow>
{
    public void Configure(EntityTypeBuilder<WalletTransactionRow> builder)
    {
        builder.ToTable("wallet_transactions");
        builder.Property(row => row.Id).ValueGeneratedNever();
        builder.Property(row => row.OwnerId).HasMaxLength(100);
        builder.Property(row => row.Type).HasMaxLength(30);
        builder.Property(row => row.Amount).HasPrecision(14, 2);
        builder.Property(row => row.BalanceAfter).HasPrecision(14, 2);
        builder.Property(row => row.Currency).HasMaxLength(3).IsFixedLength();
        builder.Property(row => row.CardBrand).HasMaxLength(30);
        builder.Property(row => row.CardLast4).HasMaxLength(4).IsFixedLength();
        builder.Property(row => row.Status).HasMaxLength(20);
        builder.Property(row => row.Reason).HasMaxLength(100);
        builder.HasIndex(row => new { row.OwnerId, row.OccurredAt });
    }
}
