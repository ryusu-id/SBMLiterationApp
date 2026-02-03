using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace PureTCOWebApp.Features.ReadingRecommendationModule.Domain;

public class ReadingRecommendationEntityConfiguration : IEntityTypeConfiguration<ReadingRecommendation>
{
    public void Configure(EntityTypeBuilder<ReadingRecommendation> builder)
    {
        builder.ToTable("mt_reading_recommendation");

        builder.HasKey(e => e.Id)
            .HasName("pk_mt_reading_recommendation");

        builder.Property(e => e.Id)
            .HasColumnName("id");

        builder.Ignore(e => e.CreateByStr);
        builder.Ignore(e => e.UpdateByStr);

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

        builder.Property(e => e.Exp)
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

        builder.HasIndex(e => e.ISBN)
            .HasDatabaseName("ix_reading_recommendation_isbn");
    }
}
