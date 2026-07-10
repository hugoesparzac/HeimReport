using HeimReport.Api.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HeimReport.Api.Data.Configurations;

public class QuestionOptionConfiguration : IEntityTypeConfiguration<QuestionOption>
{
    public void Configure(EntityTypeBuilder<QuestionOption> builder)
    {
        builder.ToTable("QuestionOptions");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.QuestionId)
            .IsRequired();
        builder.Property(x => x.Text)
            .IsRequired()
            .HasMaxLength(200);
        builder.Property(x => x.Value)
            .HasMaxLength(50);
        builder.Property(x => x.OrderIndex)
            .IsRequired();

        builder.HasOne(x => x.Question)
            .WithMany()
            .HasForeignKey(x => x.QuestionId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}