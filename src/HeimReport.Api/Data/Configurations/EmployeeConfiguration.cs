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
            .HasMaxLength(100);
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
            .HasDefaultValue(EmployeeStatus.Active);
        builder.Property(x => x.CountryId)
            .IsRequired();
        builder.Property(x => x.DepartmentId)
            .IsRequired();
        builder.Property(x => x.PositionId)
            .IsRequired();
        builder.Property(x => x.Username)
            .HasMaxLength(255);
        builder.Property(x => x.NormalizedUsername)
            .HasMaxLength(255);
        builder.HasIndex(x => x.NormalizedEmail)
            .IsUnique();
        builder.HasIndex(x => new { x.NationalId, x.CountryId })
            .IsUnique();
        builder.HasIndex(x => x.NormalizedUsername)
            .IsUnique()
            .HasFilter("\"NormalizedUsername\" IS NOT NULL");
        builder.HasOne(p => p.Country)
            .WithMany()
            .HasForeignKey(p => p.CountryId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(p => p.Department)
            .WithMany()
            .HasForeignKey(p => p.DepartmentId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(p => p.Position)
            .WithMany()
            .HasForeignKey(p => p.PositionId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(x => x.Manager)
            .WithMany(m => m.DirectReports)
            .HasForeignKey(x => x.ManagerId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}