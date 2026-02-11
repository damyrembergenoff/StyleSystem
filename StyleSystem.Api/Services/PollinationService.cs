using StyleSystem.Api.Abstractions;

namespace StyleSystem.Api.Services;

public class PollinationService : IPollinationService
{
    public Task<string> SaveImageAsync(byte[] imageBytes, string recommendationId, int index, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<List<string>> SaveMultipleImagesAsync(List<byte[]> images, string recommendationId, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }
}