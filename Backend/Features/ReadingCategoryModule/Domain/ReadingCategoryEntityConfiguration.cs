using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PureTCOWebApp.Features.ReadingCategoryModule.Domain;

namespace PureTCOWebApp.Features.ReadingCategoryModule;

public partial class ReadingCategoryConfiguration : IEntityTypeConfiguration<ReadingCategory>
{
    public void Configure(EntityTypeBuilder<ReadingCategory> builder)
    {
        builder.ToTable("mt_reading_category");

        builder.HasKey(e => e.Id)
            .HasName("pk_mt_reading_category");

        builder.Property(e => e.Id)
            .HasColumnName("id");

        builder.Ignore(e => e.CreateByStr);
        builder.Ignore(e => e.UpdateByStr);

        builder.Property(e => e.CategoryName)
            .IsRequired()
            .HasMaxLength(100)
            .IsUnicode(false)
            .HasColumnName("category_name");

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

    partial void OnConfigurePartial(EntityTypeBuilder<ReadingCategory> builder);
}
