using Microsoft.Extensions.Options;
using StyleSystem.Api.Abstractions;
using StyleSystem.Api.Options;

namespace StyleSystem.Api.Services;

public class GroqAiService(
    HttpClient httpClient, 
    IOptions<GroqOptions> options) : ITextAiService
{

    public async Task<string> GenerateRecommendationAsync(string prompt, CancellationToken cancellationToken = default)
    {
        var request = new
        {
            model = options.Value.Model,
            messages = new[]
            {
                new
                {
                    role = "system",
                    content = "You are a professional fashion stylist with expertise in body types, color theory, and occasion-appropriate styling."
                },
                new
                {
                    role = "user",
                    content = prompt
                }
            },
            temperature = 0.7
        };
        
        var response = await httpClient.PostAsJsonAsync(
            options.Value.BaseUrl,
            request,
            cancellationToken: cancellationToken
        );
        
        if (!response.IsSuccessStatusCode)
        {
            var error = await response.Content.ReadAsStringAsync(cancellationToken);
            throw new Exception($"Groq API error: {error}");
        }

        var result = await response.Content.ReadFromJsonAsync<GroqResponse>(cancellationToken: cancellationToken);
        return result?.Choices?.FirstOrDefault()?.Message?.Content 
            ?? throw new Exception("No response from Groq AI");
    }

    // Response models
    private class GroqResponse
    {
        public List<Choice>? Choices { get; set; }
    }

    private class Choice
    {
        public Message? Message { get; set; }
    }

    private class Message
    {
        public string? Content { get; set; }
    }
}