namespace HeimReport.Api.Security;

public static class SecurityServiceExtensions
{
    public static IServiceCollection AddSecurityServices(this IServiceCollection services)
    {
        services.AddScoped<IPasswordHasher, BCryptPasswordHasher>();
        services.AddScoped<ITokenHasher, Sha256TokenHasher>();
        
        return services;
    }
}