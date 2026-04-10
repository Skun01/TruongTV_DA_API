using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

public class GrammarRelationConfiguration : IEntityTypeConfiguration<GrammarRelation>
{
    public void Configure(EntityTypeBuilder<GrammarRelation> builder)
    {
        builder.ToTable("grammar_relations");

        builder.HasKey(r => new { r.GrammarId, r.RelatedId, r.RelationType });

        builder.HasOne(r => r.Grammar)
            .WithMany()
            .HasForeignKey(r => r.GrammarId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(r => r.Related)
            .WithMany()
            .HasForeignKey(r => r.RelatedId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
