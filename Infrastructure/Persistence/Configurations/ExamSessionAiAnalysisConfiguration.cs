using Domain.Entities;
using Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

public class ExamSessionAiAnalysisConfiguration : IEntityTypeConfiguration<ExamSessionAiAnalysis>
{
    public void Configure(EntityTypeBuilder<ExamSessionAiAnalysis> builder)
    {
        builder.ToTable("exam_session_ai_analyses");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.ExamSessionId)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(x => x.UserId)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(x => x.PromptVersion)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(x => x.Model)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(x => x.Status)
            .HasConversion<string>()
            .HasMaxLength(20)
            .HasDefaultValue(ExamSessionAiAnalysisStatus.Completed);

        builder.Property(x => x.InputHash)
            .IsRequired()
            .HasMaxLength(128);

        builder.Property(x => x.OutputJson)
            .IsRequired()
            .HasColumnType("text");

        builder.Property(x => x.ErrorMessage)
            .IsRequired(false)
            .HasMaxLength(1000);

        builder.Property(x => x.LatencyMs)
            .IsRequired(false);

        builder.Property(x => x.TriggerType)
            .HasConversion<string>()
            .HasMaxLength(20)
            .HasDefaultValue(ExamSessionAiAnalysisTriggerType.AutoGenerate);

        builder.Property(x => x.TriggerReason)
            .IsRequired(false)
            .HasMaxLength(50);

        builder.Property(x => x.CreatedAt)
            .HasDefaultValueSql("now()");

        builder.Property(x => x.UpdatedAt)
            .IsRequired(false);

        builder.HasOne(x => x.ExamSession)
            .WithMany(x => x.AiAnalyses)
            .HasForeignKey(x => x.ExamSessionId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.User)
            .WithMany(x => x.ExamSessionAiAnalyses)
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(x => new { x.ExamSessionId, x.PromptVersion, x.InputHash })
            .HasDatabaseName("idx_exam_session_ai_analyses_session_prompt_hash");

        builder.HasIndex(x => new { x.UserId, x.CreatedAt })
            .HasDatabaseName("idx_exam_session_ai_analyses_user_created");

        builder.HasIndex(x => new { x.UserId, x.TriggerType, x.CreatedAt })
            .HasDatabaseName("idx_exam_session_ai_analyses_user_trigger_created");
    }
}
