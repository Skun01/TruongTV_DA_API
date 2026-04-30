using Domain.Entities;
using Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

public class ExamSessionConfiguration : IEntityTypeConfiguration<ExamSession>
{
    public void Configure(EntityTypeBuilder<ExamSession> builder)
    {
        builder.ToTable("exam_sessions");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.ExamId)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(x => x.UserId)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(x => x.StartedAt)
            .IsRequired();

        builder.Property(x => x.SubmittedAt)
            .IsRequired(false);

        builder.Property(x => x.ExpiresAt)
            .IsRequired();

        builder.Property(x => x.Status)
            .HasConversion<string>()
            .HasMaxLength(20)
            .HasDefaultValue(ExamSessionStatus.InProgress);

        builder.Property(x => x.TotalScore)
            .IsRequired(false);

        builder.Property(x => x.IsPassed)
            .IsRequired(false);

        builder.Property(x => x.CreatedAt)
            .HasDefaultValueSql("now()");

        builder.Property(x => x.UpdatedAt)
            .IsRequired(false);

        builder.HasOne(x => x.Exam)
            .WithMany(x => x.ExamSessions)
            .HasForeignKey(x => x.ExamId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.User)
            .WithMany(x => x.ExamSessions)
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(x => new { x.UserId, x.ExamId })
            .HasDatabaseName("idx_exam_sessions_user_exam");

        builder.HasIndex(x => new { x.Status, x.ExpiresAt })
            .HasDatabaseName("idx_exam_sessions_status_expires");
    }
}
