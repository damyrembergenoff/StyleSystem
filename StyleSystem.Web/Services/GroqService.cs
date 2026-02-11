using System.Net.Http.Json;
using StyleSystem.Shared.Dtos;
using StyleSystem.Web.Abstractions;

namespace StyleSystem.Web.Services;

public class GroqService(IHttpClientFactory httpClientFactory) : IGroqService
{
    private string key = "api/user/";
    private HttpClient publicHttp = httpClientFactory.CreateClient("Public");
    public async ValueTask<FashionRecommendationResponse> GetFashionRecommmendationsAsync(FashionRequest fashionRequest, CancellationToken cancellationToken = default)
    {
        try
        {
            var result = await publicHttp.PostAsJsonAsync(key + "recommend", fashionRequest, cancellationToken);

            if(result.IsSuccessStatusCode)
            {
                var response = await result.Content.ReadFromJsonAsync<FashionRecommendationResponse>(cancellationToken: cancellationToken);
                
                return response!;
            }
            
            return new FashionRecommendationResponse
            {
                Recommendation = string.Empty
            };
        }
        catch(Exception)
        {
            return new FashionRecommendationResponse
            {
                Recommendation = string.Empty
            };
        }
    }
}