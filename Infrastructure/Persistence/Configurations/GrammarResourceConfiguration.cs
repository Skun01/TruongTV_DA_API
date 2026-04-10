using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

public class GrammarResourceConfiguration : IEntityTypeConfiguration<GrammarResource>
{
    public void Configure(EntityTypeBuilder<GrammarResource> builder)
    {
        builder.ToTable("grammar_resources");

        builder.HasKey(r => r.Id);

        builder.Property(r => r.Title).IsRequired();
        builder.Property(r => r.Url).IsRequired();

        builder.HasOne(r => r.Card)
            .WithMany(c => c.GrammarResources)
            .HasForeignKey(r => r.CardId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(r => r.CardId);
    }
}
