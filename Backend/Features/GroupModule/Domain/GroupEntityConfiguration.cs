using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace PureTCOWebApp.Features.GroupModule.Domain;

public class GroupEntityConfiguration : IEntityTypeConfiguration<Group>
{
    public void Configure(EntityTypeBuilder<Group> builder)
    {
        builder.ToTable("groups");

        builder.HasKey(e => e.Id).HasName("pk_groups");

        builder.Property(e => e.Id).HasColumnName("id");

        builder.Property(e => e.Name)
            .IsRequired()
            .HasMaxLength(150)
            .IsUnicode(false)
            .HasColumnName("name");

        builder.Property(e => e.Description)
            .HasColumnType("text")
            .HasColumnName("description");

        builder.Ignore(e => e.CreateByStr);
        builder.Ignore(e => e.UpdateByStr);

        builder.Property(e => e.Status).HasDefaultValue(0).HasColumnName("status");
        builder.Property(e => e.CreateBy).HasColumnName("create_by");
        builder.Property(e => e.CreateTime)
            .HasDefaultValueSql("now()")
            .HasColumnType("timestamp with time zone")
            .HasColumnName("create_time");
        builder.Property(e => e.UpdateBy).HasColumnName("update_by");
        builder.Property(e => e.UpdateTime)
            .HasColumnType("timestamp with time zone")
            .HasColumnName("update_time");

        builder.HasMany(e => e.Members)
            .WithOne(e => e.Group)
            .HasForeignKey(e => e.GroupId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public class GroupMemberEntityConfiguration : IEntityTypeConfiguration<GroupMember>
{
    public void Configure(EntityTypeBuilder<GroupMember> builder)
    {
        builder.ToTable("group_members");

        builder.HasKey(e => e.Id).HasName("pk_group_members");

        builder.Property(e => e.Id).HasColumnName("id");

        builder.Property(e => e.GroupId).HasColumnName("group_id");

        builder.Property(e => e.UserId).HasColumnName("user_id");

        builder.Ignore(e => e.CreateByStr);
        builder.Ignore(e => e.UpdateByStr);

        builder.Property(e => e.Status).HasDefaultValue(0).HasColumnName("status");
        builder.Property(e => e.CreateBy).HasColumnName("create_by");
        builder.Property(e => e.CreateTime)
            .HasDefaultValueSql("now()")
            .HasColumnType("timestamp with time zone")
            .HasColumnName("create_time");
        builder.Property(e => e.UpdateBy).HasColumnName("update_by");
        builder.Property(e => e.UpdateTime)
            .HasColumnType("timestamp with time zone")
            .HasColumnName("update_time");

        // One user → one group only
        builder.HasIndex(e => e.UserId)
            .IsUnique()
            .HasDatabaseName("uq_group_members_user_id");

        builder.HasOne(e => e.User)
            .WithMany()
            .HasForeignKey(e => e.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.Group)
            .WithMany(g => g.Members)
            .HasForeignKey(e => e.GroupId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
