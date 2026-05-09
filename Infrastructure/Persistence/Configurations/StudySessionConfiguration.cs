using Domain.Entities;
using Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

public class StudySessionConfiguration : IEntityTypeConfiguration<StudySession>
{
    public void Configure(EntityTypeBuilder<StudySession> builder)
    {
        builder.ToTable("study_sessions");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.UserId)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(x => x.DeckId)
            .IsRequired(false)
            .HasMaxLength(50);

        builder.Property(x => x.Mode)
            .HasConversion<string>()
            .HasMaxLength(30)
            .IsRequired();

        builder.Property(x => x.FlashcardFront)
            .HasConversion<string>()
            .HasMaxLength(20)
            .HasDefaultValue(FlashcardContentType.Title)
            .HasSentinel((FlashcardContentType)(-1));

        builder.Property(x => x.FlashcardBack)
            .HasConversion<string>()
            .HasMaxLength(20)
            .HasDefaultValue(FlashcardContentType.Summary)
            .HasSentinel((FlashcardContentType)(-1));

        builder.Property(x => x.MultipleChoiceQuestion)
            .HasConversion<string>()
            .HasMaxLength(30)
            .HasDefaultValue(MultipleChoiceQuestionType.TitleToSummary);

        builder.Property(x => x.ShuffleOptions)
            .HasDefaultValue(true);

        builder.Property(x => x.SelectedFolderIds)
            .HasColumnType("text[]")
            .HasDefaultValueSql("'{}'::text[]");

        builder.Property(x => x.CardIds)
            .HasColumnType("text[]")
            .HasDefaultValueSql("'{}'::text[]");

        builder.Property(x => x.CompletedCardIds)
            .HasColumnType("text[]")
            .HasDefaultValueSql("'{}'::text[]");

        builder.Property(x => x.RetryCardIds)
            .HasColumnType("text[]")
            .HasDefaultValueSql("'{}'::text[]");

        builder.Property(x => x.SkippedCardIds)
            .HasColumnType("text[]")
            .HasDefaultValueSql("'{}'::text[]");

        builder.Property(x => x.CorrectCount)
            .HasDefaultValue(0);

        builder.Property(x => x.IncorrectCount)
            .HasDefaultValue(0);

        builder.Property(x => x.CreatedAt)
            .HasDefaultValueSql("now()");

        builder.Property(x => x.UpdatedAt)
            .IsRequired(false);

        builder.HasOne(x => x.User)
            .WithMany(x => x.StudySessions)
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.Deck)
            .WithMany()
            .HasForeignKey(x => x.DeckId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasIndex(x => new { x.UserId, x.CompletedAt })
            .HasDatabaseName("idx_study_sessions_user_completed");
    }
}
