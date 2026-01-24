using PureTCOWebApp.Features.ReadingResourceModule.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PureTCOWebApp.Features.ReadingResourceModule.Domain;

namespace PureTCOWebApp.Features.ReadingResourceModule;

public partial class ReadingResourceConfiguration : IEntityTypeConfiguration<ReadingResourceBase>
{
    public void Configure(EntityTypeBuilder<ReadingResourceBase> builder)
    {
        builder.ToTable("mt_reading_resource");

        builder.HasKey(e => e.Id)
            .HasName("pk_mt_reading_resource");

        builder.HasIndex(e => e.ISBN, "ix_mt_reading_resource_isbn");

        builder.Property(e => e.Id)
            .HasColumnName("id");

        builder.Ignore(e => e.CreateByStr);
        builder.Ignore(e => e.UpdateByStr);

        builder.Property(e => e.UserId)
            .IsRequired()
            .HasColumnName("user_id");

        builder.Property(e => e.Title)
            .IsRequired()
            .HasMaxLength(200)
            .IsUnicode(false)
            .HasColumnName("title");

        builder.Property(e => e.ISBN)
            .IsRequired()
            .HasMaxLength(50)
            .IsUnicode(false)
            .HasColumnName("isbn");

        builder.Property(e => e.ReadingCategory)
            .IsRequired()
            .HasMaxLength(100)
            .IsUnicode(false)
            .HasColumnName("reading_category");

        builder.Property(e => e.Authors)
            .IsRequired()
            .HasMaxLength(300)
            .IsUnicode(false)
            .HasColumnName("authors");

        builder.Property(e => e.PublishYear)
            .IsRequired()
            .HasMaxLength(10)
            .IsUnicode(false)
            .HasColumnName("publish_year");

        builder.Property(e => e.Page)
            .IsRequired()
            .HasColumnName("page");

        builder.Property(e => e.ResourceLink)
            .HasMaxLength(500)
            .IsUnicode(false)
            .HasColumnName("resource_link");

        builder.Property(e => e.CoverImageUri)
            .HasMaxLength(500)
            .IsUnicode(false)
            .HasColumnName("cover_image_uri");

        builder.Property(e => e.CssClass)
            .IsRequired()
            .HasMaxLength(100)
            .IsUnicode(false)
            .HasColumnName("css_class");

        builder.Property<string>("resource_type")
            .HasMaxLength(20)
            .IsUnicode(false)
            .HasDefaultValue("BOOK");

        builder
            .HasDiscriminator<string>("resource_type")
            .HasValue<Book>("BOOK")
            .HasValue<JournalPaper>("JOURNAL")
            .IsComplete();

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

        builder.HasMany(e => e.ReadingReports)
            .WithOne(d => d.ReadingResource)
            .HasForeignKey(d => d.ReadingResourceId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(e => e.StreakExps)
            .WithOne(d => d.ReadingResource)
            .HasForeignKey("reading_resource_id")
            .OnDelete(DeleteBehavior.Cascade);

        OnConfigurePartial(builder);
    }

    partial void OnConfigurePartial(EntityTypeBuilder<ReadingResourceBase> builder);
}

public partial class ReadingReportConfiguration : IEntityTypeConfiguration<ReadingReport>
{
    public void Configure(EntityTypeBuilder<ReadingReport> builder)
    {
        builder.ToTable("mt_reading_report");

        builder.HasKey(e => e.Id)
            .HasName("pk_mt_reading_report");

        builder.Property(e => e.Id)
            .HasColumnName("id");

        builder.Ignore(e => e.CreateByStr);
        builder.Ignore(e => e.UpdateByStr);

        builder.Property(e => e.UserId)
            .IsRequired()
            .HasColumnName("user_id");

        builder.Property(e => e.ReadingResourceId)
            .IsRequired()
            .HasColumnName("reading_resource_id");

        builder.Property(e => e.ReportDate)
            .IsRequired()
            .HasColumnType("timestamp with time zone")
            .HasColumnName("report_date");

        builder.Property(e => e.CurrentPage)
            .IsRequired()
            .HasColumnName("current_page");

        builder.Property(e => e.Insight)
            .IsRequired()
            .HasMaxLength(1000)
            .IsUnicode(false)
            .HasColumnName("insight");

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

        builder.HasIndex(e => e.UserId)
            .HasDatabaseName("ix_reading_report_user_id");

        builder.HasIndex(e => e.ReadingResourceId)
            .HasDatabaseName("ix_reading_report_resource_id");

        OnConfigurePartial(builder);
    }

    partial void OnConfigurePartial(EntityTypeBuilder<ReadingReport> builder);
}

public partial class StreakExpConfiguration : IEntityTypeConfiguration<StreakExp>
{
    public void Configure(EntityTypeBuilder<StreakExp> builder)
    {
        builder.ToTable("mt_streak_exp");

        builder.HasKey(e => e.Id)
            .HasName("pk_mt_streak_exp");

        builder.Property(e => e.Id)
            .HasColumnName("id");

        builder.Ignore(e => e.CreateByStr);
        builder.Ignore(e => e.UpdateByStr);

        builder.Property(e => e.UserId)
            .IsRequired()
            .HasColumnName("user_id");

        builder.Property(e => e.StreakDateFrom)
            .IsRequired()
            .HasColumnName("streak_date_from");

        builder.Property(e => e.Duration)
            .IsRequired()
            .HasColumnName("duration");

        builder.Property(e => e.Exp)
            .IsRequired()
            .HasColumnName("exp");

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

        builder.HasIndex(e => e.UserId)
            .HasDatabaseName("ix_streak_exp_user_id");

        OnConfigurePartial(builder);
    }

    partial void OnConfigurePartial(EntityTypeBuilder<StreakExp> builder);
}

