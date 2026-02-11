using System.Net.Http.Headers;
using Microsoft.Extensions.Options;
using StyleSystem.Api.Abstractions;
using StyleSystem.Api.Dtos;
using StyleSystem.Api.Options;

namespace StyleSystem.Api.Services;

public class GroqService(HttpClient httpClient, IOptions<GroqOptions> options) : IGroqService
{
    public async ValueTask<string?> GetFashionRecommmendationsAsync(string prompt, CancellationToken cancellationToken = default)
    {
        var request = new GroqRequest
        {
            Model = options.Value.Model,
            Messages = [
                new GroqMessage { Role = "system", Content = "You are a helpful clothing recommendation assistant. Always answer in Uzbek."},
                new GroqMessage { Role = "user", Content = prompt }
            ]
        };

        httpClient.DefaultRequestHeaders.Authorization = 
            new AuthenticationHeaderValue("Bearer", options.Value.ApiKey);
        
        var response = await httpClient.PostAsJsonAsync(
            options.Value.BaseUrl,
            request,
            cancellationToken
        );

        if(!response.IsSuccessStatusCode)
        {
            var error = await response.Content.ReadAsStringAsync(cancellationToken);
            throw new Exception($"Groq API error: {error}");
        }

        var groqResponse = await response.Content.ReadFromJsonAsync<GroqResponse>(cancellationToken);

        return groqResponse?.Choices.FirstOrDefault()?.Message?.Content;
    }
}