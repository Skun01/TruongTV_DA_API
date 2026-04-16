using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

public class DeckFolderConfiguration : IEntityTypeConfiguration<DeckFolder>
{
    public void Configure(EntityTypeBuilder<DeckFolder> builder)
    {
        builder.ToTable("deck_folders");

        builder.HasKey(df => df.Id);

        builder.Property(df => df.Title)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(df => df.Description)
            .IsRequired()
            .HasDefaultValue(string.Empty);

        builder.Property(df => df.Position)
            .IsRequired();

        builder.Property(df => df.CardsCount)
            .HasDefaultValue(0);

        builder.Property(df => df.CreatedAt)
            .HasDefaultValueSql("now()");

        builder.Property(df => df.UpdatedAt)
            .IsRequired(false);

        builder.HasOne(df => df.Deck)
            .WithMany(d => d.Folders)
            .HasForeignKey(df => df.DeckId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasAlternateKey(df => new { df.DeckId, df.Id });

        builder.HasIndex(df => new { df.DeckId, df.Position })
            .HasDatabaseName("idx_deck_folders_deck_position");
    }
}
