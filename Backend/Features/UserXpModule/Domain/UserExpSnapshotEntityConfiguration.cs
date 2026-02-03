using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace PureTCOWebApp.Features.UserXpModule.Domain;

public class UserExpSnapshotEntityConfiguration : IEntityTypeConfiguration<UserExpSnapshot>
{
    public void Configure(EntityTypeBuilder<UserExpSnapshot> builder)
    {
        builder.ToTable("user_exp_snapshots");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasColumnName("id");

        builder.Property(x => x.UserId)
            .HasColumnName("user_id")
            .IsRequired();

        builder.Property(x => x.SnapshotSeq)
            .HasColumnName("snapshot_seq")
            .IsRequired();

        builder.Property(x => x.LastEventSeq)
            .HasColumnName("last_event_seq")
            .IsRequired();

        builder.Property(x => x.Exp)
            .HasColumnName("exp")
            .HasColumnType("decimal(18,2)")
            .IsRequired();

        builder.HasIndex(x => new { x.UserId, x.SnapshotSeq })
            .IsUnique();
    }
}
