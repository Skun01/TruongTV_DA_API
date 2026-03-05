using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

public class ReviewLogConfiguration : IEntityTypeConfiguration<ReviewLog>
{
    public void Configure(EntityTypeBuilder<ReviewLog> builder)
    {
        builder.ToTable("ReviewLogs");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.IsCorrect).IsRequired();
        builder.Property(x => x.UserAnswer).HasMaxLength(500);
        builder.Property(x => x.IsGhost).IsRequired().HasDefaultValue(false);

        builder.HasOne(r => r.User)
               .WithMany()
               .HasForeignKey(r => r.UserId)
               .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(r => r.ExampleSentence)
               .WithMany()
               .HasForeignKey(r => r.ExampleSentenceId)
               .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(r => r.CardProgress)
               .WithMany()
               .HasForeignKey(r => r.CardProgressId)
               .OnDelete(DeleteBehavior.NoAction);
    }
}
