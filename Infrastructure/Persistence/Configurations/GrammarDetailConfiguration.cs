using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System.Text.Json;

namespace Infrastructure.Persistence.Configurations;

public class GrammarDetailConfiguration : IEntityTypeConfiguration<GrammarDetail>
{
    public void Configure(EntityTypeBuilder<GrammarDetail> builder)
    {
        builder.ToTable("grammar_details");

        builder.HasKey(g => g.CardId);

        builder.HasOne(g => g.Card)
            .WithOne(c => c.GrammarDetail)
            .HasForeignKey<GrammarDetail>(g => g.CardId)
            .OnDelete(DeleteBehavior.Cascade);

        var serializerOptions = new JsonSerializerOptions(JsonSerializerDefaults.Web);

        var structuresComparer = new ValueComparer<List<Domain.ValueObjects.GrammarStructureItem>>(
            (left, right) => JsonSerializer.Serialize(left ?? new(), serializerOptions) == JsonSerializer.Serialize(right ?? new(), serializerOptions),
            value => JsonSerializer.Serialize(value ?? new(), serializerOptions).GetHashCode(),
            value => JsonSerializer.Deserialize<List<Domain.ValueObjects.GrammarStructureItem>>(
                    JsonSerializer.Serialize(value ?? new(), serializerOptions),
                    serializerOptions) ?? new());

        builder.Property(g => g.Structures)
            .HasColumnName("structures")
            .HasColumnType("jsonb")
            .HasConversion(
                value => JsonSerializer.Serialize(value ?? new(), serializerOptions),
                value => JsonSerializer.Deserialize<List<Domain.ValueObjects.GrammarStructureItem>>(value, serializerOptions) ?? new())
            .Metadata.SetValueComparer(structuresComparer);
    }
}
