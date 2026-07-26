namespace HeimReport.Api.Data.Seeding;

public interface IDataSeeder
{
    Task SeedAsync(CancellationToken cancellationToken = default);
}