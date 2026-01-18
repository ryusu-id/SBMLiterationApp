using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace PureTCOWebApp.Features.TestModule;

public partial class TestItemEntityConfiguration : IEntityTypeConfiguration<TestItem>
{
    public void Configure(EntityTypeBuilder<TestItem> builder)
    {
        builder.ToTable("mt_test_item");

        builder.HasKey(e => e.Id)
            .HasName("pk_mt_test_item");

        builder.HasIndex(e => e.Name, "uq_mt_test_item_name")
            .IsUnique();

        builder.Property(e => e.Id)
            .HasColumnName("id");

        builder.Ignore(e => e.CreateByStr);
        builder.Ignore(e => e.UpdateByStr);

        builder.Property(e => e.Name)
            .IsRequired()
            .HasMaxLength(100)
            .IsUnicode(false)
            .HasColumnName("name");

        builder.Property(e => e.Description)
            .HasMaxLength(500)
            .IsUnicode(false)
            .HasColumnName("description");

        builder.Property(e => e.Price)
            .IsRequired()
            .HasColumnType("decimal(18,2)")
            .HasColumnName("price");

        builder.Property(e => e.IsActive)
            .HasDefaultValue(true)
            .HasColumnName("is_active");

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

    partial void OnConfigurePartial(EntityTypeBuilder<TestItem> builder);
}