using StyleSystem.Shared.Dtos;

namespace StyleSystem.Web.Abstractions;

public interface IGroqService
{
    ValueTask<FashionRecommendationResponse> GetFashionRecommmendationsAsync(FashionRequest fashionRequest, CancellationToken cancellationToken = default);
}