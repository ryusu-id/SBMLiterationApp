using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace PureTCOWebApp.Features.StreakModule.Domain;

public class StreakLogEntityConfiguration : IEntityTypeConfiguration<StreakLog>
{
    public void Configure(EntityTypeBuilder<StreakLog> builder)
    {
        builder.ToTable("streak_logs");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasColumnName("id");

        builder.Property(x => x.UserId)
            .HasColumnName("user_id")
            .IsRequired();

        builder.Property(x => x.StreakDate)
            .HasColumnName("streak_date")
            .IsRequired();

        builder.HasIndex(x => new { x.UserId, x.StreakDate })
            .IsUnique();
    }
}
