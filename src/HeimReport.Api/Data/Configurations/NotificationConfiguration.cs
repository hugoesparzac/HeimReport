using HeimReport.Api.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HeimReport.Api.Data.Configurations;

public class NotificationConfiguration : IEntityTypeConfiguration<Notification>
{
    public void Configure(EntityTypeBuilder<Notification> builder)
    {
        builder.ToTable("Notifications");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.EmployeeId)
            .IsRequired();
        builder.Property(x => x.Type)
            .IsRequired();
        builder.Property(x => x.SentAt)
            .IsRequired();
        builder.Property(x => x.Channel)
            .IsRequired();

        builder.HasIndex(x => new { x.EmployeeId, x.SentAt });
        
        builder.HasOne(x => x.Employee)
            .WithMany()
            .HasForeignKey(x => x.EmployeeId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(x => x.SurveyInstance)
            .WithMany()
            .HasForeignKey(x => x.SurveyInstanceId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}