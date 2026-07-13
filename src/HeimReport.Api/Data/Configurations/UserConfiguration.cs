using HeimReport.Api.Entities;
using HeimReport.Api.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HeimReport.Api.Data.Configurations;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("Users");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.EmployeeId)
            .IsRequired();
        builder.Property(x => x.Username)
            .IsRequired()
            .HasMaxLength(50);
        builder.Property(x => x.NormalizedUsername)
            .IsRequired()
            .HasMaxLength(50);
        builder.Property(x => x.PasswordHash)
            .IsRequired()
            .HasMaxLength(60);
        builder.Property(x => x.Role)
            .IsRequired();
        builder.Property(x => x.IsEmailVerified)
            .IsRequired()
            .HasDefaultValue(false);
        builder.Property(x => x.EmailVerificationTokenHash)
            .HasMaxLength(255);
        builder.Property(x => x.IsActive)
            .IsRequired()
            .HasDefaultValue(true);
        builder.Property(x => x.CreatedAt)
            .IsRequired();
        builder.Property(x => x.PreferredLanguage)
            .IsRequired()
            .HasDefaultValue(Language.English)
            .HasSentinel(Language.English);

        builder.HasIndex(x => x.EmployeeId)
            .IsUnique();
        builder.HasIndex(x => x.NormalizedUsername)
            .IsUnique();

        builder.HasOne(x => x.Employee)
            .WithMany()
            .HasForeignKey(x => x.EmployeeId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
