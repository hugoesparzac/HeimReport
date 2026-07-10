using HeimReport.Api.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HeimReport.Api.Data.Configurations;

public class MlModelVersionConfiguration : IEntityTypeConfiguration<MlModelVersion>
{
    public void Configure(EntityTypeBuilder<MlModelVersion> builder)
    {
        builder.ToTable("MlModelVersions");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Algorithm)
            .IsRequired()
            .HasMaxLength(100);
        builder.Property(x => x.TrainedAt)
            .IsRequired();
        builder.Property(x => x.Metrics)
            .HasMaxLength(500);
        builder.Property(x => x.IsActive)
            .IsRequired()
            .HasDefaultValue(true);
    }
}