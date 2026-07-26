using System.Text.Json;
using Bogus;
using HeimReport.Api.Data.Seeding.Models;
using HeimReport.Api.Entities;
using HeimReport.Api.Enums;
using HeimReport.Api.Security;
using Microsoft.EntityFrameworkCore;

namespace HeimReport.Api.Data.Seeding;

public sealed partial class DataSeeder(
    ApplicationDbContext context,
    IPasswordHasher passwordHasher,
    IWebHostEnvironment environment,
    ILogger<DataSeeder> logger) : IDataSeeder
{
    private const string SeedPassword = "Seed@12345";
    private const string SeedEmailDomain = "heimreport-demo.com";
    private const int EmployeesPerCountry = 50;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    private int _globalCounter = 0;

    private static readonly Dictionary<string, string> CountryLocales = new()
    {
        ["Mexico"] = "es_MX",
        ["Colombia"] = "es",
        ["United States"] = "en_US"
    };

    public async Task SeedAsync(CancellationToken cancellationToken = default)
    {
        if (!environment.IsDevelopment())
        {
            LogSeedingSkippedNotDevelopment();
            return;
        }

        if (await context.Countries.AnyAsync(cancellationToken))
        {
            LogSeedingSkippedDataExists();
            return;
        }

        LogSeedingStarted();

        var countries = await SeedCountriesAsync(cancellationToken);
        var departments = await SeedDepartmentsAsync(cancellationToken);
        var positions = await SeedPositionsAsync(cancellationToken);

        var hrManagerPosition = positions.First(p => p.Title == "HR Manager");
        var directorPosition = positions.First(p => p.Title == "Director");
        var regularPositions = positions.Where(p => p.Title is "Operator" or "Analyst" or "Senior Specialist" or "Coordinator").ToList();

        var credentialsLog = new List<string>();

        foreach (var country in countries)
        {
            await SeedCountryWorkforceAsync(
                country, departments, regularPositions, hrManagerPosition, directorPosition,
                credentialsLog, cancellationToken);
        }

        await context.SaveChangesAsync(cancellationToken);

        PrintSeedCredentials(credentialsLog);

        LogSeedingCompleted();
    }

    // ===================== CATALOGS =====================

    private async Task<List<Country>> SeedCountriesAsync(CancellationToken cancellationToken)
    {
        var items = await ReadSeedFileAsync<CountrySeedItem>("countries.json", cancellationToken);

        var entities = items.ConvertAll(i => new Country { Name = i.Name, IsActive = true });
        await context.Countries.AddRangeAsync(entities, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);

        return entities;
    }

    private async Task<List<Department>> SeedDepartmentsAsync(CancellationToken cancellationToken)
    {
        var items = await ReadSeedFileAsync<DepartmentSeedItem>("departments.json", cancellationToken);

        var entities = items.ConvertAll(i => new Department { Name = i.Name, IsActive = true });
        await context.Departments.AddRangeAsync(entities, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);

        return entities;
    }

    private async Task<List<Position>> SeedPositionsAsync(CancellationToken cancellationToken)
    {
        var items = await ReadSeedFileAsync<PositionSeedItem>("positions.json", cancellationToken);

        var entities = items.ConvertAll(i => new Position
        {
            Title = i.Title,
            CareerLevel = (CareerLevel)i.CareerLevel,
            IsCritical = i.IsCritical,
            IsActive = true
        });

        await context.Positions.AddRangeAsync(entities, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);

        return entities;
    }

    private async Task<List<T>> ReadSeedFileAsync<T>(string fileName, CancellationToken cancellationToken)
    {
        var path = Path.Combine(environment.ContentRootPath, "Data", "Seeding", "SeedData", fileName);
        var json = await File.ReadAllTextAsync(path, cancellationToken);

        return JsonSerializer.Deserialize<List<T>>(json, JsonOptions)
            ?? throw new InvalidOperationException($"Seed file '{fileName}' could not be parsed or was empty.");
    }

    // ===================== EMPLOYEES AND USERS =====================

    private async Task SeedCountryWorkforceAsync(
        Country country,
        List<Department> departments,
        List<Position> regularPositions,
        Position hrManagerPosition,
        Position directorPosition,
        List<string> credentialsLog,
        CancellationToken cancellationToken)
    {
        var locale = CountryLocales.GetValueOrDefault(country.Name, "en");
        var hrDepartment = departments.First(d => d.Name == "Human Resources");

        var adminEmployee = CreateEmployee(locale, country, hrDepartment, directorPosition, managerId: null);
        context.Employees.Add(adminEmployee);
        await context.SaveChangesAsync(cancellationToken);

        AddInitialJobHistory(adminEmployee);

        var adminUsername = $"admin.{SanitizeForUsername(country.Name)}";
        var adminUser = CreateUser(adminEmployee.Id, adminUsername, SystemRole.Admin);
        context.Users.Add(adminUser);

        credentialsLog.Add($"Admin ({country.Name}): {adminUsername} / {SeedPassword}");

        for (var i = 1; i <= 2; i++)
        {
            var hrEmployee = CreateEmployee(locale, country, hrDepartment, hrManagerPosition, managerId: adminEmployee.Id);
            context.Employees.Add(hrEmployee);
            await context.SaveChangesAsync(cancellationToken);

            AddInitialJobHistory(hrEmployee);

            var hrUsername = $"hr.{SanitizeForUsername(country.Name)}{i}";
            var hrUser = CreateUser(hrEmployee.Id, hrUsername, SystemRole.HR);
            context.Users.Add(hrUser);

            credentialsLog.Add($"HR {i} ({country.Name}): {hrUsername} / {SeedPassword}");
        }

        var possibleManagers = await context.Employees
            .Where(e => e.CountryId == country.Id)
            .Select(e => e.Id)
            .ToListAsync(cancellationToken);

        var employeeFaker = new Faker(locale);

        for (var i = 0; i < EmployeesPerCountry; i++)
        {
            var department = employeeFaker.PickRandom(departments);
            var position = employeeFaker.PickRandom(regularPositions);
            var managerId = employeeFaker.PickRandom(possibleManagers);

            var employee = CreateEmployee(locale, country, department, position, managerId);
            context.Employees.Add(employee);
        }

        await context.SaveChangesAsync(cancellationToken);

        var newRegularEmployees = await context.Employees
            .Where(e => e.CountryId == country.Id && e.PositionId != hrManagerPosition.Id && e.PositionId != directorPosition.Id)
            .ToListAsync(cancellationToken);

        foreach (var employee in newRegularEmployees.Where(e => !possibleManagers.Contains(e.Id)))
        {
            AddInitialJobHistory(employee);
        }
    }

    private Employee CreateEmployee(
        string locale, Country country, Department department, Position position, int? managerId)
    {
        var faker = new Faker(locale);
        var firstName = faker.Name.FirstName();
        var lastName = faker.Name.LastName();

        _globalCounter++;

        var email = $"{Sanitize(firstName)}.{Sanitize(lastName)}{_globalCounter}@{SeedEmailDomain}".ToLowerInvariant();
        var nationalId = $"SEED-{Sanitize(country.Name).ToUpperInvariant()}-{_globalCounter:D5}";

        var birthDate = DateTime.SpecifyKind(faker.Date.Past(30, DateTime.UtcNow.AddYears(-22)), DateTimeKind.Utc);
        var hireDate = DateTime.SpecifyKind(faker.Date.Past(5), DateTimeKind.Utc);

        return new Employee
        {
            FirstName = firstName,
            LastName = lastName,
            Email = email,
            NormalizedEmail = email.ToUpperInvariant(),
            NationalId = nationalId,
            BirthDate = birthDate,
            HireDate = hireDate,
            ContractType = ContractType.Permanent,
            Status = EmployeeStatus.Active,
            CurrentSalary = faker.Random.Decimal(12000, 90000),
            CountryId = country.Id,
            DepartmentId = department.Id,
            PositionId = position.Id,
            ManagerId = managerId,
            CreatedAt = DateTime.UtcNow
        };
    }

    private void AddInitialJobHistory(Employee employee)
    {
        context.EmployeeJobHistories.Add(new EmployeeJobHistory
        {
            EmployeeId = employee.Id,
            DepartmentId = employee.DepartmentId,
            PositionId = employee.PositionId,
            ManagerId = employee.ManagerId,
            Salary = employee.CurrentSalary,
            StartDate = employee.HireDate,
            ChangeReason = null,
            CreatedAt = DateTime.UtcNow
        });
    }

    private User CreateUser(int employeeId, string username, SystemRole role)
    {
        var normalizedUsername = username.ToUpperInvariant();

        return new User
        {
            EmployeeId = employeeId,
            Username = username,
            NormalizedUsername = normalizedUsername,
            PasswordHash = passwordHasher.Hash(SeedPassword),
            Role = role,
            IsEmailVerified = true,
            IsActive = true,
            PreferredLanguage = Language.English,
            CreatedAt = DateTime.UtcNow
        };
    }

    private static string Sanitize(string value) =>
        new([.. value.Where(char.IsLetterOrDigit)]);

    private static string SanitizeForUsername(string countryName) =>
        Sanitize(countryName).ToLowerInvariant();

    private static void PrintSeedCredentials(List<string> credentialsLog)
    {
        Console.WriteLine();
        Console.WriteLine("=== Seeded Users (Development Only) ===");
        foreach (var line in credentialsLog)
        {
            Console.WriteLine(line);
        }
        Console.WriteLine("========================================");
        Console.WriteLine();
    }

    [LoggerMessage(Level = LogLevel.Information, Message = "Database already contains data. Skipping seeding.")]
    private partial void LogSeedingSkippedDataExists();

    [LoggerMessage(Level = LogLevel.Information, Message = "Not running in Development environment. Skipping seeding.")]
    private partial void LogSeedingSkippedNotDevelopment();

    [LoggerMessage(Level = LogLevel.Information, Message = "Database seeding started.")]
    private partial void LogSeedingStarted();

    [LoggerMessage(Level = LogLevel.Information, Message = "Database seeding completed successfully.")]
    private partial void LogSeedingCompleted();
}