using HeimReport.Api.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HeimReport.Api.Data.Configurations;

public class QuestionConfiguration : IEntityTypeConfiguration<Question>
{
    public void Configure(EntityTypeBuilder<Question> builder)
    {
        builder.ToTable("Questions");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.SurveyTemplateId)
            .IsRequired();
        builder.Property(x => x.Text)
            .IsRequired()
            .HasMaxLength(500);
        builder.Property(x => x.QuestionType)
            .IsRequired();
        builder.Property(x => x.OrderIndex)
            .IsRequired();
        
        builder.HasOne(x => x.SurveyTemplate)
            .WithMany()
            .HasForeignKey(x => x.SurveyTemplateId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}