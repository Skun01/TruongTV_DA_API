using Domain.Entities;
using Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

public class DeckConfiguration : IEntityTypeConfiguration<Deck>
{
    public void Configure(EntityTypeBuilder<Deck> builder)
    {
        builder.ToTable("decks");

        builder.HasKey(d => d.Id);

        builder.Property(d => d.CreatedBy)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(d => d.Title)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(d => d.Description)
            .IsRequired()
            .HasDefaultValue(string.Empty);

        builder.Property(d => d.CoverImageUrl)
            .IsRequired(false);

        builder.Property(d => d.Visibility)
            .HasConversion<string>()
            .HasMaxLength(20)
            .HasDefaultValue(DeckVisibility.Public);

        builder.Property(d => d.Status)
            .HasConversion<string>()
            .HasMaxLength(20)
            .HasDefaultValue(PublishStatus.Draft);

        builder.Property(d => d.IsOfficial)
            .HasDefaultValue(false);

        builder.Property(d => d.CardsCount)
            .HasDefaultValue(0);

        builder.Property(d => d.FoldersCount)
            .HasDefaultValue(0);

        builder.Property(d => d.CreatedAt)
            .HasDefaultValueSql("now()");

        builder.Property(d => d.UpdatedAt)
            .IsRequired(false);

        builder.HasOne(d => d.Creator)
            .WithMany(u => u.CreatedDecks)
            .HasForeignKey(d => d.CreatedBy)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(d => d.ForkedFrom)
            .WithMany(d => d.Forks)
            .HasForeignKey(d => d.ForkedFromId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(d => d.Type)
            .WithMany(dt => dt.Decks)
            .HasForeignKey(d => d.TypeId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasIndex(d => d.TypeId)
            .HasDatabaseName("idx_decks_type_id");

        builder.HasIndex(d => d.ForkedFromId)
            .HasDatabaseName("idx_decks_forked_from");

        builder.HasIndex(d => new { d.Visibility, d.Status })
            .HasDatabaseName("idx_decks_visibility_status");

        builder.HasIndex(d => new { d.CreatedBy, d.CreatedAt })
            .HasDatabaseName("idx_decks_owner_created_at");
    }
}
