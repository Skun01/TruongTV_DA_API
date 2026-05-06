using Domain.Entities;
using Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System.Text.Json;

namespace Infrastructure.Persistence.Configurations;

public class ConversationMessageConfiguration : IEntityTypeConfiguration<ConversationMessage>
{
    public void Configure(EntityTypeBuilder<ConversationMessage> builder)
    {
        builder.ToTable("conversation_messages");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.ConversationId)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(x => x.Sender)
            .HasConversion<string>()
            .HasMaxLength(10)
            .HasDefaultValue(MessageSender.User);

        builder.Property(x => x.Text)
            .IsRequired()
            .HasMaxLength(2000);

        var jsonOptions = new JsonSerializerOptions(JsonSerializerDefaults.Web);

        var stringListComparer = new ValueComparer<List<string>>(
            (left, right) => JsonSerializer.Serialize(left ?? new(), jsonOptions) == JsonSerializer.Serialize(right ?? new(), jsonOptions),
            value => JsonSerializer.Serialize(value ?? new(), jsonOptions).GetHashCode(),
            value => JsonSerializer.Deserialize<List<string>>(JsonSerializer.Serialize(value ?? new(), jsonOptions), jsonOptions) ?? new());

        builder.Property(x => x.Suggestions)
            .IsRequired(false)
            .HasColumnType("jsonb")
            .HasConversion(
                value => JsonSerializer.Serialize(value ?? new(), jsonOptions),
                value => JsonSerializer.Deserialize<List<string>>(value, jsonOptions) ?? new())
            .Metadata.SetValueComparer(stringListComparer);

        builder.Property(x => x.GrammarPoints)
            .IsRequired(false)
            .HasColumnType("jsonb")
            .HasConversion(
                value => JsonSerializer.Serialize(value ?? new(), jsonOptions),
                value => JsonSerializer.Deserialize<List<string>>(value, jsonOptions) ?? new())
            .Metadata.SetValueComparer(stringListComparer);

        var vocabComparer = new ValueComparer<List<ExtractedVocabulary>>(
            (left, right) => JsonSerializer.Serialize(left ?? new(), jsonOptions) == JsonSerializer.Serialize(right ?? new(), jsonOptions),
            value => JsonSerializer.Serialize(value ?? new(), jsonOptions).GetHashCode(),
            value => JsonSerializer.Deserialize<List<ExtractedVocabulary>>(JsonSerializer.Serialize(value ?? new(), jsonOptions), jsonOptions) ?? new());

        builder.Property(x => x.NewVocabulary)
            .IsRequired(false)
            .HasColumnType("jsonb")
            .HasConversion(
                value => JsonSerializer.Serialize(value ?? new(), jsonOptions),
                value => JsonSerializer.Deserialize<List<ExtractedVocabulary>>(value, jsonOptions) ?? new())
            .Metadata.SetValueComparer(vocabComparer);

        builder.Property(x => x.CreatedAt)
            .IsRequired();

        builder.Property(x => x.UpdatedAt)
            .IsRequired(false);

        builder.HasOne(x => x.Conversation)
            .WithMany(x => x.Messages)
            .HasForeignKey(x => x.ConversationId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(x => x.ConversationId)
            .HasDatabaseName("idx_conversation_messages_conversation");
    }
}
