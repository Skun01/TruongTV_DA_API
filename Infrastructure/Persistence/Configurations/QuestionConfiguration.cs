using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

public class QuestionConfiguration : IEntityTypeConfiguration<Question>
{
    public void Configure(EntityTypeBuilder<Question> builder)
    {
        builder.ToTable("questions");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.GroupId)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(x => x.QuestionText)
            .IsRequired()
            .HasMaxLength(5000);

        builder.Property(x => x.ImageUrl)
            .HasMaxLength(512)
            .IsRequired(false);

        builder.Property(x => x.ImageCaption)
            .HasMaxLength(500)
            .IsRequired(false);

        builder.Property(x => x.Explanation)
            .HasMaxLength(5000)
            .IsRequired(false);

        builder.Property(x => x.Score)
            .IsRequired()
            .HasDefaultValue(1);

        builder.Property(x => x.OrderIndex)
            .IsRequired();

        builder.Property(x => x.CreatedAt)
            .HasDefaultValueSql("now()");

        builder.Property(x => x.UpdatedAt)
            .IsRequired(false);

        builder.HasOne(x => x.Group)
            .WithMany(x => x.Questions)
            .HasForeignKey(x => x.GroupId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(x => new { x.GroupId, x.OrderIndex })
            .HasDatabaseName("idx_questions_group_order");
    }
}
