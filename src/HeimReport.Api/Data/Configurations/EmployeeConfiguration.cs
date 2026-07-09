using HeimReport.Api.Entities;
using HeimReport.Api.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HeimReport.Api.Data.Configurations;

public class EmployeeConfiguration : IEntityTypeConfiguration<Employee>
{
    public void Configure(EntityTypeBuilder<Employee> builder)
    {
        builder.ToTable("Employees");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.FirstName)
            .IsRequired()
            .HasMaxLength(50);
        builder.Property(x => x.LastName)
            .IsRequired()
            .HasMaxLength(100);
        builder.Property(x => x.Email)
            .IsRequired()
            .HasMaxLength(100);
        builder.Property(x => x.NormalizedEmail)
            .IsRequired()
            .HasMaxLength(100);
        builder.Property(x => x.NationalId)
            .IsRequired()
            .HasMaxLength(50);
        builder.Property(x => x.HireDate)
            .IsRequired();
        builder.Property(x => x.ContractType)
            .IsRequired();
        builder.Property(x => x.Status)
            .IsRequired()
            .HasDefaultValue(EmployeeStatus.Active)
            .HasSentinel(EmployeeStatus.Active);
        builder.Property(x => x.CountryId)
            .IsRequired();
        builder.Property(x => x.DepartmentId)
            .IsRequired();
        builder.Property(x => x.PositionId)
            .IsRequired();
        builder.Property(x => x.CreatedAt)
            .IsRequired();
        
        builder.HasIndex(x => x.NormalizedEmail)
            .IsUnique();
        builder.HasIndex(x => new { x.NationalId, x.CountryId })
            .IsUnique();
        builder.HasIndex(x => x.Status);
        builder.HasIndex(x => x.DepartmentId);
        builder.HasIndex(x => x.PositionId);
        builder.HasIndex(x => x.ManagerId);
        builder.HasIndex(x => x.HireDate);
        
        builder.HasOne(x => x.Country)
            .WithMany()
            .HasForeignKey(x => x.CountryId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(x => x.Department)
            .WithMany()
            .HasForeignKey(x => x.DepartmentId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(x => x.Position)
            .WithMany()
            .HasForeignKey(x => x.PositionId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(x => x.Manager)
            .WithMany(x => x.DirectReports)
            .HasForeignKey(x => x.ManagerId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}