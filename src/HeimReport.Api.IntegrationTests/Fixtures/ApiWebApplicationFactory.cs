using HeimReport.Api.Data;
using HeimReport.Api.Email;
using HeimReport.Api.Enums;
using HeimReport.Api.Storage;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Moq;

namespace HeimReport.Api.IntegrationTests.Fixtures;

/// <summary>
/// Hosts the complete API in memory, pointing to the real Postgres container
/// provided by PostgreSqlContainerFixture, with external dependencies
/// (email, photo storage) replaced by test doubles.
/// </summary>
public class ApiWebApplicationFactory(string connectionString) : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        // "Testing" instead of "Development" prevents the DataSeeder from running
        // automatically (guarded by IsDevelopment()) and avoids any unwanted
        // Development-conditional behavior in tests.
        builder.UseEnvironment("Testing");

        builder.ConfigureAppConfiguration((_, config) =>
        {
            // Overrides the real connection string with the test container's connection string
            config.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:DefaultConnection"] = connectionString
            });
        });

        builder.ConfigureServices(services =>
        {
            // Replaces real email sending with a mock
            var emailSenderDescriptor = services.Single(d => d.ServiceType == typeof(IEmailSender));
            services.Remove(emailSenderDescriptor);

            var emailSenderMock = new Mock<IEmailSender>();
            emailSenderMock
                .Setup(e => e.SendEmailVerificationAsync(
                    It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Language>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);
            emailSenderMock
                .Setup(e => e.SendTemporaryPasswordAsync(
                    It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Language>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            services.AddSingleton(emailSenderMock.Object);

            // Replaces real Claudinary photo storage with a mock
            var photoStorageDescriptor = services.Single(d => d.ServiceType == typeof(IPhotoStorageService));
            services.Remove(photoStorageDescriptor);

            var photoStorageMock = new Mock<IPhotoStorageService>();
            photoStorageMock
                .Setup(p => p.UploadAsync(It.IsAny<Stream>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new PhotoUploadResult("https://fake.test/photo.jpg", "fake-public-id"));
            photoStorageMock
                .Setup(p => p.DeleteAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            services.AddSingleton(photoStorageMock.Object);
        });
    }

    /// <summary>
    /// Clears all data tables (preserving the schema) so that each
    /// test starts with an empty database without needing to recreate the container.
    /// </summary>
    public async Task ResetDatabaseAsync()
    {
        using var scope = Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        // TRUNCATE ... RESTART IDENTITY CASCADE resets auto-increment IDs
        // and handles foreign keys in a single statement, regardless of order.
        await context.Database.ExecuteSqlRawAsync("""
            TRUNCATE TABLE
                "AuditLogs", "RefreshTokens", "Users",
                "EmployeeJobHistories", "Employees",
                "Positions", "Departments", "Countries"
            RESTART IDENTITY CASCADE;
            """);
    }
}