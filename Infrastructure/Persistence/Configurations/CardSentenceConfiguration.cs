using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

public class CardSentenceConfiguration : IEntityTypeConfiguration<CardSentence>
{
    public void Configure(EntityTypeBuilder<CardSentence> builder)
    {
        builder.ToTable("card_sentences");
        
        builder.HasKey(cs => new { cs.CardId, cs.SentenceId });
        
        builder.HasOne(cs => cs.Card)
            .WithMany(c => c.CardSentences)
            .HasForeignKey(cs => cs.CardId)
            .OnDelete(DeleteBehavior.Cascade);
            
        builder.HasOne(cs => cs.Sentence)
            .WithMany(s => s.CardSentences)
            .HasForeignKey(cs => cs.SentenceId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
