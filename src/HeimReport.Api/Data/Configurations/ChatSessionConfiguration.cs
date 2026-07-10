using HeimReport.Api.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HeimReport.Api.Data.Configurations;

public class ChatSessionConfiguration : IEntityTypeConfiguration<ChatSession>
{
    public void Configure(EntityTypeBuilder<ChatSession> builder)
    {
        builder.ToTable("ChatSessions");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.SurveyInstanceId)
            .IsRequired();
        builder.Property(x => x.StartedAt)
            .IsRequired();
        builder.Property(x => x.Status)
            .IsRequired();
        
        builder.HasOne(x => x.SurveyInstance)
            .WithMany()
            .HasForeignKey(x => x.SurveyInstanceId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}