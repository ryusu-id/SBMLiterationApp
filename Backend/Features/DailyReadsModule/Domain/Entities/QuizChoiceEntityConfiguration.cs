using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace PureTCOWebApp.Features.DailyReadsModule.Domain.Entities;

public partial class QuizChoiceConfiguration : IEntityTypeConfiguration<QuizChoice>
{
    public void Configure(EntityTypeBuilder<QuizChoice> builder)
    {
        builder.ToTable("mt_quiz_choice");

        builder.HasKey(e => e.Id).HasName("pk_mt_quiz_choice");

        builder.Property(e => e.Id).HasColumnName("id");

        builder.Property<int>("QuizQuestionId")
            .IsRequired()
            .HasColumnName("quiz_question_id");

        builder.Ignore(e => e.CreateByStr);
        builder.Ignore(e => e.UpdateByStr);

        builder.Property(e => e.Choice)
            .IsRequired()
            .HasMaxLength(1)
            .HasColumnName("choice");

        builder.Property(e => e.Answer)
            .IsRequired()
            .HasColumnName("answer");

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

    partial void OnConfigurePartial(EntityTypeBuilder<QuizChoice> builder);
}
