using HeimReport.Api.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HeimReport.Api.Data.Configurations;

public class AttritionPredictionConfiguration : IEntityTypeConfiguration<AttritionPrediction>
{
    public void Configure(EntityTypeBuilder<AttritionPrediction> builder)
    {
        builder.ToTable("AttritionPredictions");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.EmployeeId)
            .IsRequired();
        builder.Property(x => x.MlModelVersionId)
            .IsRequired();
        builder.Property(x => x.PredictionDate)
            .IsRequired();
        builder.Property(x => x.RiskScore)
            .IsRequired();
        builder.Property(x => x.RiskLevel)
            .IsRequired();

        builder.HasIndex(x => new { x.EmployeeId, x.PredictionDate });

        builder.HasOne(x => x.Employee)
            .WithMany()
            .HasForeignKey(x => x.EmployeeId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(x => x.MlModelVersion)
            .WithMany()
            .HasForeignKey(x => x.MlModelVersionId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}