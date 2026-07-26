namespace HeimReport.Api.Storage;

public interface IPhotoStorageService
{
    Task<PhotoUploadResult> UploadAsync(Stream fileStream, string fileName, CancellationToken cancellationToken = default);
    Task DeleteAsync(string publicId, CancellationToken cancellationToken = default);
}