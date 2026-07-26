namespace HeimReport.Api.Storage;

public static class PhotoStorageServiceExtensions
{
    public static IServiceCollection AddPhotoStorage(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddOptions<CloudinaryOptions>()
            .Bind(configuration.GetSection(CloudinaryOptions.SectionName))
            .ValidateDataAnnotations()
            .ValidateOnStart();

        services.AddScoped<IPhotoStorageService, CloudinaryPhotoStorageService>();

        return services;
    }
}