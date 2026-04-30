using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

public class SessionSectionScoreConfiguration : IEntityTypeConfiguration<SessionSectionScore>
{
    public void Configure(EntityTypeBuilder<SessionSectionScore> builder)
    {
        builder.ToTable("session_section_scores");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.SessionId)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(x => x.SectionId)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(x => x.Score)
            .IsRequired();

        builder.Property(x => x.MaxScore)
            .IsRequired();

        builder.Property(x => x.PassScore)
            .IsRequired();

        builder.Property(x => x.IsPassed)
            .IsRequired();

        builder.Property(x => x.CreatedAt)
            .HasDefaultValueSql("now()");

        builder.Property(x => x.UpdatedAt)
            .IsRequired(false);

        builder.HasOne(x => x.Session)
            .WithMany(x => x.SectionScores)
            .HasForeignKey(x => x.SessionId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.Section)
            .WithMany(x => x.SessionSectionScores)
            .HasForeignKey(x => x.SectionId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(x => new { x.SessionId, x.SectionId })
            .IsUnique()
            .HasDatabaseName("idx_session_section_scores_session_section");
    }
}
