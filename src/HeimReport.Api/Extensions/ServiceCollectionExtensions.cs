using HeimReport.Api.Services.Countries;
using HeimReport.Api.Services.Departments;
using HeimReport.Api.Services.Positions;
using HeimReport.Api.Services.Users;

namespace HeimReport.Api.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddScoped<ICountryService, CountryService>();
        services.AddScoped<IDepartmentService, DepartmentService>();
        services.AddScoped<IPositionService, PositionService>();
        services.AddScoped<IUserService, UserService>();

        return services;
    }
}
