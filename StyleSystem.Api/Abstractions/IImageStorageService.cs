namespace StyleSystem.Api.Abstractions;

public interface IImageStorageService
{
    Task<string> SaveImageAsync(byte[] imageBytes, CancellationToken cancellationToken = default);
    Task DeleteImageAsync(string imageUrl, CancellationToken cancellationToken = default);
}
