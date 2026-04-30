using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

public class SessionAnswerConfiguration : IEntityTypeConfiguration<SessionAnswer>
{
    public void Configure(EntityTypeBuilder<SessionAnswer> builder)
    {
        builder.ToTable("session_answers");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.SessionId)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(x => x.QuestionId)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(x => x.SelectedOptionId)
            .HasMaxLength(50)
            .IsRequired(false);

        builder.Property(x => x.AnsweredAt)
            .IsRequired();

        builder.Property(x => x.CreatedAt)
            .HasDefaultValueSql("now()");

        builder.Property(x => x.UpdatedAt)
            .IsRequired(false);

        builder.HasOne(x => x.Session)
            .WithMany(x => x.Answers)
            .HasForeignKey(x => x.SessionId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.Question)
            .WithMany(x => x.SessionAnswers)
            .HasForeignKey(x => x.QuestionId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.SelectedOption)
            .WithMany()
            .HasForeignKey(x => x.SelectedOptionId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasIndex(x => new { x.SessionId, x.QuestionId })
            .IsUnique()
            .HasDatabaseName("idx_session_answers_session_question");
    }
}
