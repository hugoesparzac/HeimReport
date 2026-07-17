using HeimReport.Api.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HeimReport.Api.Data.Configurations;

public class SurveyInstanceConfiguration : IEntityTypeConfiguration<SurveyInstance>
{
    public void Configure(EntityTypeBuilder<SurveyInstance> builder)
    {
        builder.ToTable("SurveyInstances");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.EmployeeId)
            .IsRequired();
        builder.Property(x => x.SurveyTemplateId)
            .IsRequired();
        builder.Property(x => x.ScheduledDate)
            .IsRequired();
        builder.Property(x => x.Status)
            .IsRequired();
        builder.Property(x => x.Channel)
            .IsRequired();

        builder.HasIndex(x => new { x.EmployeeId, x.Status });
        builder.HasIndex(x => x.DueDate);
        builder.HasIndex(x => x.SurveyTemplateId);

        builder.HasOne(x => x.Employee)
            .WithMany()
            .HasForeignKey(x => x.EmployeeId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(x => x.SurveyTemplate)
            .WithMany()
            .HasForeignKey(x => x.SurveyTemplateId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}