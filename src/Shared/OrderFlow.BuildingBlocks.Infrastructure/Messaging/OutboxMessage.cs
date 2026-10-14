using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace OrderFlow.BuildingBlocks.Infrastructure.Messaging;

public sealed class OutboxMessage
{
    public Guid Id { get; init; }

    public string Type { get; init; } = string.Empty;

    public string Payload { get; init; } = string.Empty;

    public DateTimeOffset OccurredAt { get; init; }

    public string? CorrelationId { get; init; }

    public string? TraceParent { get; init; }

    public DateTimeOffset? ProcessedAt { get; init; }

    public int Attempts { get; init; }

    public string? LastError { get; init; }
}

internal sealed class OutboxMessageConfiguration : IEntityTypeConfiguration<OutboxMessage>
{
    public void Configure(EntityTypeBuilder<OutboxMessage> builder)
    {
        builder.ToTable("outbox_messages");
        builder.Property(message => message.Id).ValueGeneratedNever();
        builder.Property(message => message.Type).HasMaxLength(200);
        builder.Property(message => message.Payload).HasColumnType("jsonb");
        builder.Property(message => message.CorrelationId).HasMaxLength(64);
        builder.Property(message => message.TraceParent).HasMaxLength(128);
        builder.Property(message => message.LastError).HasMaxLength(2000);

        builder
            .HasIndex(message => message.OccurredAt)
            .HasFilter("processed_at IS NULL")
            .HasDatabaseName("ix_outbox_messages_unprocessed");
    }
}

public sealed class InboxMessage
{
    public Guid MessageId { get; init; }

    public string Consumer { get; init; } = string.Empty;

    public string MessageType { get; init; } = string.Empty;

    public DateTimeOffset ProcessedAt { get; init; }
}

internal sealed class InboxMessageConfiguration : IEntityTypeConfiguration<InboxMessage>
{
    public void Configure(EntityTypeBuilder<InboxMessage> builder)
    {
        builder.ToTable("inbox_messages");
        builder.HasKey(message => new { message.MessageId, message.Consumer });
        builder.Property(message => message.Consumer).HasMaxLength(200);
        builder.Property(message => message.MessageType).HasMaxLength(200);
    }
}

public static class MessagingModelBuilderExtensions
{
    public static ModelBuilder AddOutboxMessages(this ModelBuilder modelBuilder)
    {
        ArgumentNullException.ThrowIfNull(modelBuilder);
        return modelBuilder.ApplyConfiguration(new OutboxMessageConfiguration());
    }

    public static ModelBuilder AddInboxMessages(this ModelBuilder modelBuilder)
    {
        ArgumentNullException.ThrowIfNull(modelBuilder);
        return modelBuilder.ApplyConfiguration(new InboxMessageConfiguration());
    }
}
