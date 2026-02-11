namespace StyleSystem.Api.Abstractions;

public interface IGroqService
{
    ValueTask<string?> GetFashionRecommmendationsAsync(string prompt, CancellationToken cancellationToken = default);
}