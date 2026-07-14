using HeimReport.Api.Repositories;
using HeimReport.Api.Repositories.Auth;
using HeimReport.Api.Repositories.Catalogs;
using HeimReport.Api.Repositories.Catalogs.Countries;
using HeimReport.Api.Repositories.Catalogs.Departments;
using HeimReport.Api.Repositories.Catalogs.Positions;
using HeimReport.Api.Repositories.Employees;

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