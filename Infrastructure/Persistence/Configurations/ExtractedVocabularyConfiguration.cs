using Domain.Entities;
using Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

public class ExtractedVocabularyConfiguration : IEntityTypeConfiguration<ExtractedVocabulary>
{
    public void Configure(EntityTypeBuilder<ExtractedVocabulary> builder)
    {
        builder.ToTable("extracted_vocabularies");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.MessageId)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(x => x.Word)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(x => x.Reading)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(x => x.Meaning)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(x => x.Example)
            .IsRequired(false)
            .HasMaxLength(1000);

        builder.Property(x => x.JlptLevel)
            .HasConversion<string>()
            .HasMaxLength(10)
            .HasDefaultValue(JlptLevel.N5);

        builder.Property(x => x.CreatedAt)
            .HasDefaultValueSql("now()");

        builder.Property(x => x.UpdatedAt)
            .IsRequired(false);

        builder.HasIndex(x => x.MessageId)
            .HasDatabaseName("idx_extracted_vocabularies_message");
    }
}
