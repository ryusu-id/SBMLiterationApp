using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace PureTCOWebApp.Features.OutboxModule.Domain;

public class OutboxProcessedMessageEntityConfiguration : IEntityTypeConfiguration<OutboxProcessedMessage>
{
    public void Configure(EntityTypeBuilder<OutboxProcessedMessage> builder)
    {
        builder.ToTable("outbox_processed_messages");

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

        builder.Property(x => x.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        builder.Property(x => x.ProcessedAt)
            .HasColumnName("processed_at")
            .IsRequired();

        builder.HasIndex(x => x.ProcessedAt)
            .HasDatabaseName("ix_outbox_processed_messages_processed_at");
    }
}
