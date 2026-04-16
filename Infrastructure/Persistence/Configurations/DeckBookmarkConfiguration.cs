using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

public class DeckBookmarkConfiguration : IEntityTypeConfiguration<DeckBookmark>
{
    public void Configure(EntityTypeBuilder<DeckBookmark> builder)
    {
        builder.ToTable("deck_bookmarks");

        builder.HasKey(db => new { db.UserId, db.DeckId });

        builder.Property(db => db.UserId)
            .HasMaxLength(50);

        builder.Property(db => db.SavedAt)
            .HasDefaultValueSql("now()");

        builder.HasOne(db => db.User)
            .WithMany(u => u.DeckBookmarks)
            .HasForeignKey(db => db.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(db => db.Deck)
            .WithMany(d => d.Bookmarks)
            .HasForeignKey(db => db.DeckId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(db => db.DeckId)
            .HasDatabaseName("idx_deck_bookmarks_deck_id");
    }
}
