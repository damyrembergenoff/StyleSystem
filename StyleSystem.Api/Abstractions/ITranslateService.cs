using StyleSystem.Api.Dtos.Recommendations;
using StyleSystem.Shared.Dtos.Recommendations;

namespace StyleSystem.Api.Abstractions;

public interface ITranslateService
{
    ValueTask<(string Text, bool IsTranslated)> TranslateAsync(string text, CancellationToken cancellationToken = default);
    ValueTask<CreateRecommendationDto> TranslateRecommendationInputAsync(
    CreateRecommendationDto dto,
    CancellationToken cancellationToken = default);
    ValueTask<AnonymousRecommendationDto> TranslateInputAsync(
    AnonymousRecommendationDto dto,
    CancellationToken cancellationToken = default);
}