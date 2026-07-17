using HeimReport.Api.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HeimReport.Api.Data.Configurations;

public class ChatMessageConfiguration : IEntityTypeConfiguration<ChatMessage>
{
    public void Configure(EntityTypeBuilder<ChatMessage> builder)
    {
        builder.ToTable("ChatMessages");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.ChatSessionId)
            .IsRequired();
        builder.Property(x => x.Sender)
            .IsRequired();
        builder.Property(x => x.Content)
            .IsRequired()
            .HasMaxLength(2000);
        builder.Property(x => x.SentAt)
            .IsRequired();

        builder.HasOne(a => a.ChatSession)
            .WithMany()
            .HasForeignKey(a => a.ChatSessionId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(x => x.Question)
            .WithMany()
            .HasForeignKey(a => a.QuestionId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}