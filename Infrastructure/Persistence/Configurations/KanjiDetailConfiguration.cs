using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

public class KanjiDetailConfiguration : IEntityTypeConfiguration<KanjiDetail>
{
    public void Configure(EntityTypeBuilder<KanjiDetail> builder)
    {
        builder.ToTable("kanji_details");

        builder.HasKey(k => k.CardId);

        builder.HasOne(k => k.Card)
            .WithOne(c => c.KanjiDetail)
            .HasForeignKey<KanjiDetail>(k => k.CardId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Property(k => k.Kanji).IsRequired();
        builder.Property(k => k.StrokeCount).IsRequired();
        builder.Property(k => k.MeaningVi).IsRequired();

        builder.HasIndex(k => k.Kanji).IsUnique();
        builder.HasIndex(k => k.StrokeCount);
    }
}
