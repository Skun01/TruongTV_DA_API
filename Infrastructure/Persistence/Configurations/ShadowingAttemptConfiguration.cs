using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

public class ShadowingAttemptConfiguration : IEntityTypeConfiguration<ShadowingAttempt>
{
    public void Configure(EntityTypeBuilder<ShadowingAttempt> builder)
    {
        builder.ToTable("shadowing_attempts");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.UserId)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(x => x.TopicId)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(x => x.SentenceId)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(x => x.AudioAssetId)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(x => x.Locale)
            .IsRequired()
            .HasMaxLength(20)
            .HasDefaultValue("ja-JP");

        builder.Property(x => x.RecognizedText)
            .IsRequired(false);

        builder.Property(x => x.PronScore).IsRequired(false);
        builder.Property(x => x.AccuracyScore).IsRequired(false);
        builder.Property(x => x.FluencyScore).IsRequired(false);
        builder.Property(x => x.CompletenessScore).IsRequired(false);
        builder.Property(x => x.ProsodyScore).IsRequired(false);
        builder.Property(x => x.ErrorTypes).IsRequired(false);
        builder.Property(x => x.DurationMs).IsRequired(false);
        builder.Property(x => x.RawResultJson).IsRequired(false);

        builder.Property(x => x.CreatedAt)
            .HasDefaultValueSql("now()");

        builder.Property(x => x.UpdatedAt)
            .IsRequired(false);

        builder.HasOne(x => x.User)
            .WithMany()
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.Topic)
            .WithMany(x => x.Attempts)
            .HasForeignKey(x => x.TopicId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.Sentence)
            .WithMany()
            .HasForeignKey(x => x.SentenceId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.AudioAsset)
            .WithMany()
            .HasForeignKey(x => x.AudioAssetId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(x => new { x.UserId, x.CreatedAt })
            .HasDatabaseName("idx_shadowing_attempts_user_created");

        builder.HasIndex(x => new { x.TopicId, x.CreatedAt })
            .HasDatabaseName("idx_shadowing_attempts_topic_created");

        builder.HasIndex(x => new { x.UserId, x.SentenceId, x.CreatedAt })
            .HasDatabaseName("idx_shadowing_attempts_user_sentence_created");
    }
}
