using Domain.Entities;
using Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

public class AiGeneratedQuestionConfiguration : IEntityTypeConfiguration<AiGeneratedQuestion>
{
    public void Configure(EntityTypeBuilder<AiGeneratedQuestion> builder)
    {
        builder.ToTable("ai_generated_questions");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Level)
            .HasConversion<string>()
            .HasMaxLength(10)
            .IsRequired();

        builder.Property(x => x.SectionType)
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(x => x.Topic)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(x => x.QuestionGroupId)
            .HasMaxLength(50)
            .IsRequired(false);

        builder.Property(x => x.GeneratedData)
            .IsRequired()
            .HasColumnType("text");

        builder.Property(x => x.Status)
            .HasConversion<string>()
            .HasMaxLength(20)
            .HasDefaultValue(AiQuestionStatus.Pending);

        builder.Property(x => x.ReviewedBy)
            .HasMaxLength(50)
            .IsRequired(false);

        builder.Property(x => x.ReviewedAt)
            .IsRequired(false);

        builder.Property(x => x.QuestionId)
            .HasMaxLength(50)
            .IsRequired(false);

        builder.Property(x => x.CreatedBy)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(x => x.CreatedAt)
            .HasDefaultValueSql("now()");

        builder.Property(x => x.UpdatedAt)
            .IsRequired(false);

        builder.HasOne(x => x.Reviewer)
            .WithMany()
            .HasForeignKey(x => x.ReviewedBy)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(x => x.Question)
            .WithMany()
            .HasForeignKey(x => x.QuestionId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(x => x.QuestionGroup)
            .WithMany()
            .HasForeignKey(x => x.QuestionGroupId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(x => x.Creator)
            .WithMany()
            .HasForeignKey(x => x.CreatedBy)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(x => new { x.Level, x.SectionType, x.Status })
            .HasDatabaseName("idx_ai_questions_level_section_status");
    }
}
