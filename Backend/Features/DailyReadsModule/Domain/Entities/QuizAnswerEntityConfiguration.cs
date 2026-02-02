using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace PureTCOWebApp.Features.DailyReadsModule.Domain.Entities;

public partial class QuizAnswerConfiguration : IEntityTypeConfiguration<QuizAnswer>
{
    public void Configure(EntityTypeBuilder<QuizAnswer> builder)
    {
        builder.ToTable("mt_quiz_answer");

        builder.HasKey(e => e.Id).HasName("pk_mt_quiz_answer");

        builder.HasIndex(e => new { e.UserId, e.DailyReadId, e.QuestionSeq })
            .IsUnique()
            .HasDatabaseName("ix_mt_quiz_answer_user_daily_question");

        builder.Ignore(e => e.CreateByStr);
        builder.Ignore(e => e.UpdateByStr);

        builder.Property(e => e.Id).HasColumnName("id");

        builder.Property(e => e.UserId)
            .IsRequired()
            .HasColumnName("user_id");

        builder.Property(e => e.DailyReadId)
            .IsRequired()
            .HasColumnName("daily_read_id");

        builder.Property(e => e.QuestionSeq)
            .IsRequired()
            .HasColumnName("question_seq");

        builder.Property(e => e.Answer)
            .IsRequired()
            .HasMaxLength(1)
            .HasColumnName("answer");

        builder.Property(e => e.RetrySeq)
            .HasDefaultValue(0)
            .HasColumnName("retry_seq");

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

    partial void OnConfigurePartial(EntityTypeBuilder<QuizAnswer> builder);
}
