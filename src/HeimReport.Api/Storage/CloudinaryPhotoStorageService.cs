using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Microsoft.Extensions.Options;

namespace HeimReport.Api.Storage;

public sealed class CloudinaryPhotoStorageService : IPhotoStorageService
{
    private readonly Cloudinary _cloudinary;
    private readonly CloudinaryOptions _options;

    public CloudinaryPhotoStorageService(IOptions<CloudinaryOptions> options)
    {
        _options = options.Value;
        var account = new Account(_options.CloudName, _options.ApiKey, _options.ApiSecret);
        _cloudinary = new Cloudinary(account) { Api = { Secure = true } };
    }

    public async Task<PhotoUploadResult> UploadAsync(
        Stream fileStream, string fileName, CancellationToken cancellationToken = default)
    {
        var uploadParams = new ImageUploadParams
        {
            File = new FileDescription(fileName, fileStream),
            Folder = _options.Folder,

            Transformation = new Transformation().Width(500).Height(500).Crop("fill").Gravity("face"),
            Overwrite = true
        };

        var result = await _cloudinary.UploadAsync(uploadParams, cancellationToken);

        if (result.Error is not null)
        {
            throw new InvalidOperationException($"Failed to upload photo to Cloudinary: {result.Error.Message}");
        }

        return new PhotoUploadResult(result.SecureUrl.ToString(), result.PublicId);
    }

    public async Task DeleteAsync(string publicId, CancellationToken cancellationToken = default)
    {
        var deleteParams = new DeletionParams(publicId);
        await _cloudinary.DestroyAsync(deleteParams);
    }
}