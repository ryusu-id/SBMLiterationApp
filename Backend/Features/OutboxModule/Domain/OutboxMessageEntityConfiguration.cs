using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace PureTCOWebApp.Features.OutboxModule.Domain;

public class OutboxMessageEntityConfiguration : IEntityTypeConfiguration<OutboxMessage>
{
    public void Configure(EntityTypeBuilder<OutboxMessage> builder)
    {
        builder.ToTable("outbox_messages");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasColumnName("id");

        builder.Property(x => x.EventType)
            .HasColumnName("event_type")
            .HasMaxLength(500)
            .IsRequired();

        builder.Property(x => x.Payload)
            .HasColumnName("payload")
            .HasColumnType("text")
            .IsRequired();

        builder.Property(x => x.RetryCount)
            .HasColumnName("retry_count")
            .IsRequired();

        builder.Property(x => x.MaxResilienceCount)
            .HasColumnName("max_resilience_count")
            .IsRequired();

        builder.Property(x => x.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        builder.Property(x => x.LastAttemptedAt)
            .HasColumnName("last_attempted_at");

        builder.Property(x => x.LastError)
            .HasColumnName("last_error")
            .HasColumnType("text");

        builder.HasIndex(x => x.CreatedAt)
            .HasDatabaseName("ix_outbox_messages_created_at");

        builder.HasIndex(x => x.RetryCount)
            .HasDatabaseName("ix_outbox_messages_retry_count");
    }
}
