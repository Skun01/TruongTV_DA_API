using Domain.Entities;
using Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

public class CardProgressConfiguration : IEntityTypeConfiguration<CardProgress>
{
    public void Configure(EntityTypeBuilder<CardProgress> builder)
    {
        builder.ToTable("CardProgresses");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.CardType).HasConversion<string>();
        builder.Property(x => x.Status).HasConversion<string>().HasDefaultValue(CardStatus.New);
        builder.Property(x => x.SrsLevel).IsRequired().HasDefaultValue(0);
        builder.Property(x => x.CorrectStreak).IsRequired().HasDefaultValue(0);
        builder.Property(x => x.TotalReviews).IsRequired().HasDefaultValue(0);
        builder.Property(x => x.CorrectReviews).IsRequired().HasDefaultValue(0);
        builder.Property(x => x.NextExampleIndex).IsRequired().HasDefaultValue(0);

        builder.HasIndex(x => new { x.UserId, x.CardId, x.CardType }).IsUnique();
        builder.HasIndex(x => new { x.UserId, x.NextReviewAt });

        builder.HasOne(p => p.User)
               .WithMany()
               .HasForeignKey(p => p.UserId)
               .OnDelete(DeleteBehavior.Cascade);
    }
}
