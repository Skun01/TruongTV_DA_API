using Domain.Entities;
using Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

public class ShadowingTopicConfiguration : IEntityTypeConfiguration<ShadowingTopic>
{
    public void Configure(EntityTypeBuilder<ShadowingTopic> builder)
    {
        builder.ToTable("shadowing_topics");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.CreatedBy)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(x => x.Title)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(x => x.Description)
            .IsRequired()
            .HasMaxLength(2000);

        builder.Property(x => x.Level)
            .HasConversion<string>()
            .HasMaxLength(10)
            .IsRequired(false);

        builder.Property(x => x.Visibility)
            .HasConversion<string>()
            .HasMaxLength(20)
            .HasDefaultValue(DeckVisibility.Public);

        builder.Property(x => x.Status)
            .HasConversion<string>()
            .HasMaxLength(20)
            .HasDefaultValue(PublishStatus.Draft);

        builder.Property(x => x.IsOfficial)
            .HasDefaultValue(false);

        builder.Property(x => x.SentencesCount)
            .HasDefaultValue(0);

        builder.Property(x => x.CreatedAt)
            .HasDefaultValueSql("now()");

        builder.Property(x => x.UpdatedAt)
            .IsRequired(false);

        builder.HasOne(x => x.Creator)
            .WithMany()
            .HasForeignKey(x => x.CreatedBy)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(x => new { x.Visibility, x.Status })
            .HasDatabaseName("idx_shadowing_topics_visibility_status");

        builder.HasIndex(x => new { x.CreatedBy, x.CreatedAt })
            .HasDatabaseName("idx_shadowing_topics_created_by");
    }
}
