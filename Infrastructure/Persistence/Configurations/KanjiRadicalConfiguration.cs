using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

public class KanjiRadicalConfiguration : IEntityTypeConfiguration<KanjiRadical>
{
    public void Configure(EntityTypeBuilder<KanjiRadical> builder)
    {
        builder.ToTable("kanji_radicals");

        builder.HasKey(k => new { k.KanjiId, k.RadicalId });

        builder.HasOne(k => k.KanjiCard)
            .WithMany(c => c.KanjiRadicals)
            .HasForeignKey(k => k.KanjiId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(k => k.Radical)
            .WithMany(r => r.KanjiRadicals)
            .HasForeignKey(k => k.RadicalId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
