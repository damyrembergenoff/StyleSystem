using StyleSystem.Api.Dtos.Recommendations;
using StyleSystem.Shared.Dtos.Recommendations;
using StyleSystem.Shared.DTOs.Recommendations;

namespace StyleSystem.Api.Abstractions;

public interface IRecommendationService
{
    // Authenticated user uchun
    Task<RecommendationResponseDto> CreateAsync(CreateRecommendationDto dto, Guid userId, CancellationToken cancellationToken);
    Task<RecommendationResponseDto> GetByIdAsync(Guid id, Guid userId, CancellationToken cancellationToken);
    Task<IList<RecommendationResponseDto>> GetUserHistoryAsync(Guid userId, CancellationToken cancellationToken);

    // Anonymous user uchun
    Task<AnonymousRecommendationResponseDto> CreateAnonymousAsync(AnonymousRecommendationDto dto, CancellationToken cancellationToken);
}
