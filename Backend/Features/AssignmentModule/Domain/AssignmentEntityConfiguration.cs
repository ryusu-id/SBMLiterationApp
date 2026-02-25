using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PureTCOWebApp.Features.AssignmentModule.Domain;

namespace PureTCOWebApp.Features.AssignmentModule;

public class AssignmentEntityConfiguration : IEntityTypeConfiguration<Assignment>
{
    public void Configure(EntityTypeBuilder<Assignment> builder)
    {
        builder.ToTable("assignments");

        builder.HasKey(e => e.Id).HasName("pk_assignments");

        builder.Property(e => e.Id).HasColumnName("id");

        builder.Property(e => e.Title)
            .IsRequired()
            .HasMaxLength(200)
            .IsUnicode(false)
            .HasColumnName("title");

        builder.Property(e => e.Description)
            .HasColumnType("text")
            .HasColumnName("description");

        builder.Property(e => e.DueDate)
            .HasColumnType("timestamp with time zone")
            .HasColumnName("due_date");

        builder.Property(e => e.CreateTime)
            .HasDefaultValueSql("now()")
            .HasColumnType("timestamp with time zone")
            .HasColumnName("create_time");

        builder.Property(e => e.UpdateTime)
            .HasColumnType("timestamp with time zone")
            .HasColumnName("update_time");

        builder.HasMany(e => e.Submissions)
            .WithOne(e => e.Assignment)
            .HasForeignKey(e => e.AssignmentId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public class AssignmentSubmissionEntityConfiguration : IEntityTypeConfiguration<AssignmentSubmission>
{
    public void Configure(EntityTypeBuilder<AssignmentSubmission> builder)
    {
        builder.ToTable("assignment_submissions");

        builder.HasKey(e => e.Id).HasName("pk_assignment_submissions");

        builder.Property(e => e.Id).HasColumnName("id");

        builder.Property(e => e.AssignmentId).HasColumnName("assignment_id");

        builder.Property(e => e.GroupId).HasColumnName("group_id");

        builder.Property(e => e.IsCompleted)
            .HasDefaultValue(false)
            .HasColumnName("is_completed");

        builder.Property(e => e.CompletedAt)
            .HasColumnType("timestamp with time zone")
            .HasColumnName("completed_at");

        builder.Property(e => e.CreateTime)
            .HasDefaultValueSql("now()")
            .HasColumnType("timestamp with time zone")
            .HasColumnName("create_time");

        // One submission per group per assignment
        builder.HasIndex(e => new { e.AssignmentId, e.GroupId })
            .IsUnique()
            .HasDatabaseName("uq_assignment_submissions_assignment_group");

        builder.HasOne(e => e.Group)
            .WithMany()
            .HasForeignKey(e => e.GroupId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.Assignment)
            .WithMany(a => a.Submissions)
            .HasForeignKey(e => e.AssignmentId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(e => e.Files)
            .WithOne(e => e.Submission)
            .HasForeignKey(e => e.AssignmentSubmissionId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public class AssignmentSubmissionFileEntityConfiguration : IEntityTypeConfiguration<AssignmentSubmissionFile>
{
    public void Configure(EntityTypeBuilder<AssignmentSubmissionFile> builder)
    {
        builder.ToTable("assignment_submission_files");

        builder.HasKey(e => e.Id).HasName("pk_assignment_submission_files");

        builder.Property(e => e.Id).HasColumnName("id");

        builder.Property(e => e.AssignmentSubmissionId).HasColumnName("assignment_submission_id");

        builder.Property(e => e.UploadedByUserId).HasColumnName("uploaded_by_user_id");

        builder.Property(e => e.FileName)
            .IsRequired()
            .HasMaxLength(300)
            .IsUnicode(false)
            .HasColumnName("file_name");

        builder.Property(e => e.FileUri)
            .HasMaxLength(500)
            .IsUnicode(false)
            .HasColumnName("file_uri");

        builder.Property(e => e.ExternalLink)
            .HasMaxLength(500)
            .IsUnicode(false)
            .HasColumnName("external_link");

        builder.Property(e => e.UploadedAt)
            .HasDefaultValueSql("now()")
            .HasColumnType("timestamp with time zone")
            .HasColumnName("uploaded_at");

        builder.HasOne(e => e.UploadedBy)
            .WithMany()
            .HasForeignKey(e => e.UploadedByUserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.Submission)
            .WithMany(s => s.Files)
            .HasForeignKey(e => e.AssignmentSubmissionId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
