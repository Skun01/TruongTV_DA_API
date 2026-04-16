using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

public class FolderCardConfiguration : IEntityTypeConfiguration<FolderCard>
{
    public void Configure(EntityTypeBuilder<FolderCard> builder)
    {
        builder.ToTable("folder_cards");

        builder.HasKey(fc => new { fc.FolderId, fc.CardId });

        builder.Property(fc => fc.Position)
            .IsRequired();

        builder.Property(fc => fc.AddedAt)
            .HasDefaultValueSql("now()");

        builder.HasOne(fc => fc.Deck)
            .WithMany(d => d.FolderCards)
            .HasForeignKey(fc => fc.DeckId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(fc => fc.Folder)
            .WithMany(df => df.FolderCards)
            .HasForeignKey(fc => new { fc.DeckId, fc.FolderId })
            .HasPrincipalKey(df => new { df.DeckId, df.Id })
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(fc => fc.Card)
            .WithMany(c => c.FolderCards)
            .HasForeignKey(fc => fc.CardId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(fc => new { fc.DeckId, fc.CardId })
            .IsUnique()
            .HasDatabaseName("uq_folder_cards_deck_card");

        builder.HasIndex(fc => new { fc.FolderId, fc.Position })
            .HasDatabaseName("idx_folder_cards_folder_position");

        builder.HasIndex(fc => fc.CardId)
            .HasDatabaseName("idx_folder_cards_card_id");

    }
}
