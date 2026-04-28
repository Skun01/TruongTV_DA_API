using Domain.Entities;
using Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

public class UserLearningSettingsConfiguration : IEntityTypeConfiguration<UserLearningSettings>
{
    public void Configure(EntityTypeBuilder<UserLearningSettings> builder)
    {
        builder.ToTable("user_learning_settings");

        builder.HasKey(x => x.UserId);

        builder.Property(x => x.UserId)
            .HasMaxLength(50);

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

        builder.Property(x => x.CreatedAt)
            .HasDefaultValueSql("now()");

        builder.Property(x => x.UpdatedAt)
            .HasDefaultValueSql("now()");

        builder.HasOne(x => x.User)
            .WithOne(x => x.LearningSettings)
            .HasForeignKey<UserLearningSettings>(x => x.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
