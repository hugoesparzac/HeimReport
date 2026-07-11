using HeimReport.Api.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HeimReport.Api.Data.Configurations;

public class AttritionFactorConfiguration : IEntityTypeConfiguration<AttritionFactor>
{
    public void Configure(EntityTypeBuilder<AttritionFactor> builder)
    {
        builder.ToTable("AttritionFactors");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.AttritionPredictionId)
            .IsRequired();
        builder.Property(x => x.Name)
            .IsRequired()
            .HasMaxLength(150);
        builder.Property(x => x.ContributionScore)
            .IsRequired();
        
        builder.HasOne(x => x.AttritionPrediction)
            .WithMany()
            .HasForeignKey(x => x.AttritionPredictionId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}