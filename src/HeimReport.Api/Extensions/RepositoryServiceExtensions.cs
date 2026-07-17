using HeimReport.Api.Repositories;
using HeimReport.Api.Repositories.Auth;
using HeimReport.Api.Repositories.Employees;
using HeimReport.Api.Repositories.Countries;
using HeimReport.Api.Repositories.Departments;
using HeimReport.Api.Repositories.Positions;

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

        return services;
    }
}