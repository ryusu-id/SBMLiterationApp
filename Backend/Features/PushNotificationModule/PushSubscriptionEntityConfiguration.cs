using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PureTCOWebApp.Features.PushNotificationModule.Domain;

namespace PureTCOWebApp.Features.PushNotificationModule;

public class PushSubscriptionEntityConfiguration : IEntityTypeConfiguration<PushSubscription>
{
    public void Configure(EntityTypeBuilder<PushSubscription> builder)
    {
        builder.ToTable("push_subscriptions");

        builder.HasKey(e => e.Id)
            .HasName("pk_push_subscriptions");

        builder.Property(e => e.Id).HasColumnName("id");
        builder.Property(e => e.UserId).HasColumnName("user_id");
        builder.Property(e => e.Endpoint).IsRequired().HasColumnName("endpoint");
        builder.Property(e => e.P256dh).IsRequired().HasColumnName("p256dh");
        builder.Property(e => e.Auth).IsRequired().HasColumnName("auth");
        builder.Property(e => e.UserAgent).HasColumnName("user_agent");
        builder.Property(e => e.CreatedAt)
            .HasDefaultValueSql("now()")
            .HasColumnType("timestamp with time zone")
            .HasColumnName("created_at");

        // Unique per endpoint — prevents duplicate entries for same browser
        builder.HasIndex(e => e.Endpoint)
            .IsUnique()
            .HasDatabaseName("uq_push_subscriptions_endpoint");

        builder.HasOne(e => e.User)
            .WithMany()
            .HasForeignKey(e => e.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
