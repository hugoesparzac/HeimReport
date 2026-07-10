using HeimReport.Api.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HeimReport.Api.Data.Configurations;

public class SurveyTemplateConfiguration : IEntityTypeConfiguration<SurveyTemplate>
{
    public void Configure(EntityTypeBuilder<SurveyTemplate> builder)
    {
        builder.ToTable("SurveyTemplates");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Name)
            .IsRequired()
            .HasMaxLength(100);
        builder.Property(x => x.MilestoneMonths)
            .IsRequired();
        builder.Property(x => x.Description)
            .HasMaxLength(255);
        builder.Property(x => x.IsActive)
            .IsRequired();
        builder.Property(x => x.CreatedAt)
            .IsRequired();
    }
}