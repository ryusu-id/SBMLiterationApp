using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace PureTCOWebApp.Features.DailyReadsModule.Domain;

public partial class DailyReadConfiguration : IEntityTypeConfiguration<DailyRead>
{
    public void Configure(EntityTypeBuilder<DailyRead> builder)
    {
        builder.ToTable("mt_daily_read");

        builder.HasKey(e => e.Id).HasName("pk_mt_daily_read");

        builder.Property(e => e.Id).HasColumnName("id");

        builder.Ignore(e => e.CreateByStr);
        builder.Ignore(e => e.UpdateByStr);

        builder.Property(e => e.Title)
            .IsRequired()
            .HasMaxLength(200)
            .HasColumnName("title");

        builder.Property(e => e.CoverImg)
            .HasMaxLength(500)
            .HasColumnName("cover_img");

        builder.Property(e => e.Content)
            .IsRequired()
            .HasColumnName("content");

        builder.Property(e => e.Date)
            .IsRequired()
            .HasColumnName("date");

        builder.Property(e => e.Category)
            .HasMaxLength(100)
            .HasColumnName("category");

        builder.Property(e => e.Exp)
            .HasPrecision(18, 2)
            .HasDefaultValue(0)
            .HasColumnName("exp");

        builder.Property(e => e.MinimalCorrectAnswer)
            .HasDefaultValue(0)
            .HasColumnName("minimal_correct_answer");

        builder.Property(e => e.Status)
            .HasDefaultValue(0)
            .HasColumnName("status");

        builder.Property(e => e.CreateBy).HasColumnName("create_by");
        builder.Property(e => e.CreateTime)
            .HasDefaultValueSql("(now())")
            .HasColumnType("timestamp with time zone")
            .HasColumnName("create_time");

        builder.Property(e => e.UpdateBy).HasColumnName("update_by");
        builder.Property(e => e.UpdateTime)
            .HasColumnType("timestamp with time zone")
            .HasColumnName("update_time");

        OnConfigurePartial(builder);
    }

    partial void OnConfigurePartial(EntityTypeBuilder<DailyRead> builder);
}
