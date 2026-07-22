using HeimReport.Api.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace HeimReport.Api.Data.Configurations;

public class EmployeeJobHistoryConfiguration : IEntityTypeConfiguration<EmployeeJobHistory>
{
    public void Configure(EntityTypeBuilder<EmployeeJobHistory> builder)
    {
        builder.ToTable("EmployeeJobHistories");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.EmployeeId)
            .IsRequired();
        builder.Property(x => x.DepartmentId)
            .IsRequired();
        builder.Property(x => x.PositionId)
            .IsRequired();
        builder.Property(x => x.StartDate)
            .IsRequired();
        builder.Property(x => x.CreatedAt)
            .IsRequired();

        builder.HasIndex(x => x.EmployeeId);
        builder.HasIndex(x => x.StartDate);

        builder.HasOne(x => x.Employee)
            .WithMany()
            .HasForeignKey(x => x.EmployeeId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(x => x.Department)
            .WithMany()
            .HasForeignKey(x => x.DepartmentId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(x => x.Position)
            .WithMany()
            .HasForeignKey(x => x.PositionId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(x => x.Manager)
            .WithMany()
            .HasForeignKey(x => x.ManagerId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
