using StyleSystem.Api.Abstractions;

namespace StyleSystem.Api.Services;

public class LocalImageStorageService(IWebHostEnvironment environment) : IImageStorageService
{
    private const string ImageFolder = "generated-images";

    public LocalImageStorageService() : this(null!)
    {
        EnsureDirectoryExists();
    }

    public async Task<string> SaveImageAsync(byte[] imageBytes, CancellationToken cancellationToken = default)
    {
        var fileName = $"outfit_{Guid.NewGuid()}.png";
        var folderPath = Path.Combine(environment.WebRootPath, ImageFolder);
        var filePath = Path.Combine(folderPath, fileName);

        await File.WriteAllBytesAsync(filePath, imageBytes);

        return $"/{ImageFolder}/{fileName}";
    }

    public Task DeleteImageAsync(string imageUrl, CancellationToken cancellationToken = default)
    {
        var fileName = Path.GetFileName(imageUrl);
        var filePath = Path.Combine(environment.WebRootPath, ImageFolder, fileName);

        if (File.Exists(filePath))
            File.Delete(filePath);

        return Task.CompletedTask;
    }

    private void EnsureDirectoryExists()
    {
        var folderPath = Path.Combine(environment.WebRootPath ?? "wwwroot", ImageFolder);
        if (!Directory.Exists(folderPath))
            Directory.CreateDirectory(folderPath);
    }
}