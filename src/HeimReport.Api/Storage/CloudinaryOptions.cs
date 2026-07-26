namespace HeimReport.Api.Storage;

public class CloudinaryOptions
{
    public const string SectionName = "Cloudinary";
    public required string CloudName { get; init; }
    public required string ApiKey { get; init; }
    public required string ApiSecret { get; init; }
    public required string Folder { get; init; }
}