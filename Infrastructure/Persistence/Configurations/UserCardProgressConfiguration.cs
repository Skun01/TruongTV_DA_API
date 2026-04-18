using Domain.Entities;
using Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

public class UserCardProgressConfiguration : IEntityTypeConfiguration<UserCardProgress>
{
    public void Configure(EntityTypeBuilder<UserCardProgress> builder)
    {
        builder.ToTable("user_card_progress");

        builder.HasKey(x => new { x.UserId, x.CardId });

        builder.Property(x => x.SrsLevel)
            .HasConversion<string>()
            .HasMaxLength(20)
            .HasDefaultValue(SrsLevel.Level1);

        builder.Property(x => x.NextReviewAt)
            .IsRequired();

        builder.Property(x => x.LastReviewedAt)
            .IsRequired(false);

        builder.Property(x => x.ConsecutiveCorrect)
            .HasDefaultValue(0);

        builder.Property(x => x.IsMastered)
            .HasDefaultValue(false);

        builder.Property(x => x.CreatedAt)
            .HasDefaultValueSql("now()");

        builder.Property(x => x.UpdatedAt)
            .HasDefaultValueSql("now()");

        builder.HasOne(x => x.User)
            .WithMany(x => x.CardProgresses)
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.Card)
            .WithMany(x => x.UserCardProgresses)
            .HasForeignKey(x => x.CardId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.LastSentence)
            .WithMany()
            .HasForeignKey(x => x.LastSentenceId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasIndex(x => new { x.UserId, x.NextReviewAt })
            .HasDatabaseName("idx_user_card_progress_user_next_review");
    }
}
