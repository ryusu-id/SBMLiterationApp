using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace PureTCOWebApp.Features.DailyReadsModule.Domain.Entities;

public partial class QuizQuestionConfiguration : IEntityTypeConfiguration<QuizQuestion>
{
    public void Configure(EntityTypeBuilder<QuizQuestion> builder)
    {
        builder.ToTable("mt_quiz_question");

        builder.HasKey(e => e.Id).HasName("pk_mt_quiz_question");

        builder.Property(e => e.Id).HasColumnName("id");

        builder.Ignore(e => e.CreateByStr);
        builder.Ignore(e => e.UpdateByStr);

        builder.Property(e => e.DailyReadId)
            .IsRequired()
            .HasColumnName("daily_read_id");

        builder.Property(e => e.QuestionSeq)
            .IsRequired()
            .HasColumnName("question_seq");

        builder.Property(e => e.Question)
            .IsRequired()
            .HasColumnName("question");

        builder.Property(e => e.CorrectAnswer)
            .IsRequired()
            .HasMaxLength(1)
            .HasColumnName("correct_answer");

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

        builder.HasOne(e => e.DailyRead)
            .WithMany()
            .HasForeignKey(e => e.DailyReadId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(e => e.Choices)
            .WithOne()
            .HasForeignKey("QuizQuestionId")
            .OnDelete(DeleteBehavior.Cascade);

        OnConfigurePartial(builder);
    }

    partial void OnConfigurePartial(EntityTypeBuilder<QuizQuestion> builder);
}
