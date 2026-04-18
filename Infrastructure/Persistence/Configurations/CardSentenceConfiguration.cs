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

        builder.Property(cs => cs.Position)
            .IsRequired();

        builder.Property(cs => cs.BlankWord)
            .IsRequired(false);

        builder.Property(cs => cs.Hint)
            .IsRequired(false);

        builder.Property(cs => cs.AnswerList)
            .HasColumnType("text[]")
            .HasDefaultValueSql("'{}'::text[]");

        builder.HasOne(cs => cs.Card)
            .WithMany(c => c.CardSentences)
            .HasForeignKey(cs => cs.CardId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(cs => cs.Sentence)
            .WithMany(s => s.CardSentences)
            .HasForeignKey(cs => cs.SentenceId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(cs => new { cs.CardId, cs.Position })
            .HasDatabaseName("idx_card_sentences_card_position");
    }
}
