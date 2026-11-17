using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;

namespace ST10335260_PROG6212_POE_Part2.Services;

public interface IBlobStorageService
{
    Task<string> UploadAsync(string containerName, IFormFile file, CancellationToken cancellationToken = default);
}

public static class BlobContainers
{
    public const string EventBanners = "event-banners";
    public const string ProfilePictures = "profile-pictures";
}

public static class ImageValidator
{
    private const long MaxBytes = 5 * 1024 * 1024;

    private static readonly HashSet<string> AllowedTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        "image/jpeg", "image/png", "image/webp", "image/gif"
    };

    private static readonly HashSet<string> AllowedExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".jpg", ".jpeg", ".png", ".webp", ".gif"
    };

    public static string? Validate(IFormFile? file)
    {
        if (file is null || file.Length == 0)
            return "Please choose an image file to upload.";

        if (file.Length > MaxBytes)
            return "The image must be 5 MB or smaller.";

        if (!AllowedTypes.Contains(file.ContentType) || !AllowedExtensions.Contains(Path.GetExtension(file.FileName)))
            return "Only JPG, PNG, WEBP or GIF images are allowed.";

        return null;
    }
}

public class AzureBlobStorageService : IBlobStorageService
{
    private readonly BlobServiceClient _client;
    private readonly string _publicBaseUrl;

    public AzureBlobStorageService(IConfiguration configuration)
    {
        var connectionString = configuration["BlobStorage:ConnectionString"] ?? "UseDevelopmentStorage=true";
        _client = new BlobServiceClient(connectionString);

        var configuredBase = configuration["BlobStorage:PublicBaseUrl"];
        _publicBaseUrl = string.IsNullOrWhiteSpace(configuredBase)
            ? _client.Uri.ToString().TrimEnd('/')
            : configuredBase.TrimEnd('/');
    }

    public async Task<string> UploadAsync(string containerName, IFormFile file, CancellationToken cancellationToken = default)
    {
        var container = _client.GetBlobContainerClient(containerName);
        await container.CreateIfNotExistsAsync(PublicAccessType.Blob, cancellationToken: cancellationToken);

        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
        var blobName = $"{Guid.NewGuid():N}{extension}";
        var blob = container.GetBlobClient(blobName);

        await using var stream = file.OpenReadStream();
        await blob.UploadAsync(stream, new BlobUploadOptions
        {
            HttpHeaders = new BlobHttpHeaders { ContentType = file.ContentType }
        }, cancellationToken);

        return $"{_publicBaseUrl}/{containerName}/{blobName}";
    }
}
