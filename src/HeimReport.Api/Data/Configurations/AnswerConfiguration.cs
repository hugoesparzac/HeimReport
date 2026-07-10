using HeimReport.Api.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HeimReport.Api.Data.Configurations;

public class AnswerConfiguration : IEntityTypeConfiguration<Answer>
{
    public void Configure(EntityTypeBuilder<Answer> builder)
    {
        builder.ToTable("Answers");
        builder.HasKey(a => a.Id);
        builder.Property(a => a.SurveyInstanceId)
            .IsRequired();
        builder.Property(a => a.QuestionId)
            .IsRequired();
        builder.Property(a => a.RawText)
            .HasMaxLength(1000);
        builder.Property(a => a.NormalizedText)
            .HasMaxLength(100);
        builder.Property(a => a.AnsweredAt)
            .IsRequired();

        builder.HasIndex(x => new { x.SurveyInstanceId, x.QuestionId })
            .IsUnique();
        
        builder.HasOne(a => a.SurveyInstance)
            .WithMany()
            .HasForeignKey(a => a.SurveyInstanceId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(a => a.Question)
            .WithMany()
            .HasForeignKey(a => a.QuestionId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(a => a.ChatMessage)
            .WithMany()
            .HasForeignKey(a => a.ChatMessageId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}