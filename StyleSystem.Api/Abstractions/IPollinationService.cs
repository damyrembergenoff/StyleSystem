namespace StyleSystem.Api.Abstractions;

public interface IPollinationService
{
    Task<string> SaveImageAsync(
        byte[] imageBytes, 
        string recommendationId, 
        int index,
        CancellationToken cancellationToken = default);
    Task<List<string>> SaveMultipleImagesAsync(
        List<byte[]> images, 
        string recommendationId, 
        CancellationToken cancellationToken = default);
}