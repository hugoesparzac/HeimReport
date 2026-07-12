namespace HeimReport.Api.Email;

public static class EmailServiceExtensions
{
    public static IServiceCollection AddEmailSender(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddOptions<MailgunOptions>()
            .Bind(configuration.GetSection(MailgunOptions.SectionName))
            .ValidateDataAnnotations()
            .ValidateOnStart();

        services.AddScoped<IEmailSender, MailgunEmailSender>();

        return services;
    }
}