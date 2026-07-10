using HeimReport.Api.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HeimReport.Api.Data.Configurations;

public class RecommendationConfiguration : IEntityTypeConfiguration<Recommendation>
{
    public void Configure(EntityTypeBuilder<Recommendation> builder)
    {
        builder.ToTable("Recommendations");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.AttritionPredictionId)
            .IsRequired();
        builder.Property(x => x.SuggestionText)
            .IsRequired()
            .HasMaxLength(500);
        builder.Property(x => x.Category)
            .HasMaxLength(100);
        builder.Property(x => x.Status)
            .IsRequired();
        builder.Property(x => x.CreatedAt)
            .IsRequired();
        
        builder.HasOne(x => x.AttritionPrediction)
            .WithMany()
            .HasForeignKey(x => x.AttritionPredictionId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}