using Domain.Entities;
using Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

public class QuestionGroupConfiguration : IEntityTypeConfiguration<QuestionGroup>
{
    public void Configure(EntityTypeBuilder<QuestionGroup> builder)
    {
        builder.ToTable("question_groups");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.SectionId)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(x => x.PassageText)
            .IsRequired(false);

        builder.Property(x => x.AudioUrl)
            .HasMaxLength(512)
            .IsRequired(false);

        builder.Property(x => x.AudioScript)
            .IsRequired(false);

        builder.Property(x => x.Instruction)
            .IsRequired()
            .HasMaxLength(2000);

        builder.Property(x => x.OrderIndex)
            .IsRequired();

        builder.Property(x => x.MondaiType)
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired(false);

        builder.Property(x => x.CreatedAt)
            .HasDefaultValueSql("now()");

        builder.Property(x => x.UpdatedAt)
            .IsRequired(false);

        builder.HasOne(x => x.Section)
            .WithMany(x => x.QuestionGroups)
            .HasForeignKey(x => x.SectionId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(x => new { x.SectionId, x.OrderIndex })
            .HasDatabaseName("idx_question_groups_section_order");
    }
}
