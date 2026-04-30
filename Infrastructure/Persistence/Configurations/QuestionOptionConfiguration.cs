using Domain.Entities;
using Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

public class QuestionOptionConfiguration : IEntityTypeConfiguration<QuestionOption>
{
    public void Configure(EntityTypeBuilder<QuestionOption> builder)
    {
        builder.ToTable("question_options");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.QuestionId)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(x => x.Label)
            .HasConversion<string>()
            .HasMaxLength(5)
            .IsRequired();

        builder.Property(x => x.Text)
            .HasMaxLength(2000)
            .IsRequired(false);

        builder.Property(x => x.ImageUrl)
            .HasMaxLength(512)
            .IsRequired(false);

        builder.Property(x => x.OptionType)
            .HasConversion<string>()
            .HasMaxLength(20)
            .HasDefaultValue(OptionType.Text);

        builder.Property(x => x.IsCorrect)
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(x => x.CreatedAt)
            .HasDefaultValueSql("now()");

        builder.Property(x => x.UpdatedAt)
            .IsRequired(false);

        builder.HasOne(x => x.Question)
            .WithMany(x => x.Options)
            .HasForeignKey(x => x.QuestionId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(x => new { x.QuestionId, x.Label })
            .IsUnique()
            .HasDatabaseName("idx_question_options_question_label");
    }
}
