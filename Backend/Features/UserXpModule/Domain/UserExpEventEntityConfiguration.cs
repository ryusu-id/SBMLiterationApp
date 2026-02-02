using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace PureTCOWebApp.Features.UserXpModule.Domain;

public class UserExpEventEntityConfiguration : IEntityTypeConfiguration<UserExpEvent>
{
    public void Configure(EntityTypeBuilder<UserExpEvent> builder)
    {
        builder.ToTable("user_exp_events");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasColumnName("id");

        builder.Property(x => x.UserId)
            .HasColumnName("user_id")
            .IsRequired();

        builder.Property(x => x.Exp)
            .HasColumnName("exp")
            .HasColumnType("decimal(18,2)")
            .IsRequired();

        builder.Property(x => x.EventName)
            .HasColumnName("event_name")
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(x => x.RefId)
            .HasColumnName("ref_id")
            .IsRequired();

        builder.HasIndex(x => new { x.UserId });
    }
}
