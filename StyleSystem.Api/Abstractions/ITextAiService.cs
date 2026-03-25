namespace StyleSystem.Api.Abstractions;

// AI #1 - Text
public interface ITextAiService
{
    Task<string> GenerateRecommendationAsync(string prompt, CancellationToken cancellationToken = default);
}
