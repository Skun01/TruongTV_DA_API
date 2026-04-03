using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

public class VocabularyDetailConfiguration : IEntityTypeConfiguration<VocabularyDetail>
{
    public void Configure(EntityTypeBuilder<VocabularyDetail> builder)
    {
        builder.ToTable("vocabulary_details");
        
        builder.HasKey(v => v.CardId);
        
        builder.HasOne(v => v.Card)
            .WithOne(c => c.VocabularyDetail)
            .HasForeignKey<VocabularyDetail>(v => v.CardId)
            .OnDelete(DeleteBehavior.Cascade);
            
        builder.Property(v => v.Writing).IsRequired();
        
        // Map as JSONB using EF Core 8 Owned Entities
        builder.OwnsMany(v => v.Meanings, a => 
        {
            a.ToJson("meanings");
        });
    }
}
