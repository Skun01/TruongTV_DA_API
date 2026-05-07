using Domain.Entities;
using Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

public class ConversationSessionConfiguration : IEntityTypeConfiguration<ConversationSession>
{
    public void Configure(EntityTypeBuilder<ConversationSession> builder)
    {
        builder.ToTable("conversation_sessions");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.UserId)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(x => x.Scenario)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(x => x.CustomScenario)
            .IsRequired(false)
            .HasMaxLength(500);

        builder.Property(x => x.Level)
            .HasConversion<string>()
            .HasMaxLength(10)
            .HasDefaultValue(JlptLevel.N5);

        builder.Property(x => x.Status)
            .HasConversion<string>()
            .HasMaxLength(20)
            .HasDefaultValue(ConversationSessionStatus.Active);

        builder.Property(x => x.StartedAt)
            .IsRequired();

        builder.Property(x => x.CompletedAt)
            .IsRequired(false);

        builder.Property(x => x.Feedback)
            .IsRequired(false)
            .HasColumnType("text");

        builder.Property(x => x.FeedbackGeneratedAt)
            .IsRequired(false);

        builder.Property(x => x.ResultModel)
            .IsRequired(false)
            .HasMaxLength(200);

        builder.Property(x => x.ResultPromptVersion)
            .IsRequired(false)
            .HasMaxLength(100);

        builder.Property(x => x.TotalMessages)
            .HasDefaultValue(0);

        builder.Property(x => x.UserMessagesCount)
            .HasDefaultValue(0);

        builder.Property(x => x.Score)
            .HasDefaultValue(0);

        builder.Property(x => x.CreatedAt)
            .HasDefaultValueSql("now()");

        builder.Property(x => x.UpdatedAt)
            .IsRequired(false);

        builder.HasOne(x => x.User)
            .WithMany()
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(x => x.UserId)
            .HasDatabaseName("idx_conversation_sessions_user");

        builder.HasIndex(x => x.Status)
            .HasDatabaseName("idx_conversation_sessions_status");
    }
}
