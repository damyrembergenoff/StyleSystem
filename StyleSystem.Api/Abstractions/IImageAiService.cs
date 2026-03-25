namespace StyleSystem.Api.Abstractions;

// AI #2 - Image
public interface IImageAiService
{
    Task<List<byte[]>> GenerateImagesAsync(string prompt, int count = 2, CancellationToken cancellationToken = default);
}