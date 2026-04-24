using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

public class ShadowingTopicSentenceConfiguration : IEntityTypeConfiguration<ShadowingTopicSentence>
{
    public void Configure(EntityTypeBuilder<ShadowingTopicSentence> builder)
    {
        builder.ToTable("shadowing_topic_sentences");

        builder.HasKey(x => new { x.TopicId, x.SentenceId });

        builder.Property(x => x.Position)
            .IsRequired();

        builder.Property(x => x.Note)
            .HasMaxLength(1000)
            .IsRequired(false);

        builder.HasOne(x => x.Topic)
            .WithMany(x => x.TopicSentences)
            .HasForeignKey(x => x.TopicId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.Sentence)
            .WithMany()
            .HasForeignKey(x => x.SentenceId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(x => new { x.TopicId, x.Position })
            .HasDatabaseName("idx_shadowing_topic_sentences_topic_position");
    }
}
