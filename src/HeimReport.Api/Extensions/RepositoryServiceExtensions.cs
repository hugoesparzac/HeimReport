using HeimReport.Api.Repositories;
using HeimReport.Api.Repositories.AuditLogs;
using HeimReport.Api.Repositories.Countries;
using HeimReport.Api.Repositories.Departments;
using HeimReport.Api.Repositories.Employees;
using HeimReport.Api.Repositories.Positions;
using HeimReport.Api.Repositories.Users;

namespace HeimReport.Api.Extensions;

public static class RepositoryServiceExtensions
{
    public static IServiceCollection AddRepositories(this IServiceCollection services)
    {
        services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();
        services.AddScoped<IEmployeeRepository, EmployeeRepository>();
        services.AddScoped<IDepartmentRepository, DepartmentRepository>();
        services.AddScoped<IPositionRepository, PositionRepository>();
        services.AddScoped<ICountryRepository, CountryRepository>();
        services.AddScoped<IEmployeeJobHistoryRepository, EmployeeJobHistoryRepository>();
        services.AddScoped<IAuditLogRepository, AuditLogRepository>();

        return services;
    }
}