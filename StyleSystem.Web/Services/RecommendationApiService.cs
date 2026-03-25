using System.Net.Http.Json;
using StyleSystem.Api.Dtos.Recommendations;
using StyleSystem.Shared.Dtos.Recommendations;
using StyleSystem.Shared.DTOs.Recommendations;
using StyleSystem.Web.Dtos.Recommendations;

namespace StyleSystem.Web.Services;

public class RecommendationApiService(IHttpClientFactory httpClientFactory)
{
    private readonly string key = "api/recommendations/";
    private readonly HttpClient publicHttp = httpClientFactory.CreateClient("Public");
    private readonly HttpClient privateHttp = httpClientFactory.CreateClient("Private");

    public async Task<AnonymousRecommendationResponseDto?> CreateAnonymousAsync(
        AnonymousRecommendationDto dto,
        CancellationToken cancellationToken = default)
    {
        var response = await publicHttp.PostAsJsonAsync(
            $"{key}anonymous",
            dto,
            cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            var error = await response.Content.ReadAsStringAsync(cancellationToken);
            throw new Exception($"API error: {error}");
        }

        return await response.Content.ReadFromJsonAsync<AnonymousRecommendationResponseDto>(
            cancellationToken: cancellationToken);
    }

    public async Task<RecommendationResponseDto?> CreateAsync(
        CreateRecommendationDto dto,
        CancellationToken cancellationToken = default)
    {
        var response = await privateHttp.PostAsJsonAsync(key, dto, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            var error = await response.Content.ReadAsStringAsync(cancellationToken);
            throw new Exception($"API error: {error}");
        }

        return await response.Content.ReadFromJsonAsync<RecommendationResponseDto>(cancellationToken: cancellationToken);
    }

    public async Task<RecommendationResponseDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var response = await privateHttp.GetAsync($"{key}{id}", cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            var error = await response.Content.ReadAsStringAsync(cancellationToken);
            throw new Exception($"API error: {error}");
        }

        return await response.Content.ReadFromJsonAsync<RecommendationResponseDto>(cancellationToken: cancellationToken);
    }
}