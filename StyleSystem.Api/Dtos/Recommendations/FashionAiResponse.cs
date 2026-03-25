using System.Text.Json.Serialization;

namespace StyleSystem.Api.Dtos.Recommendations;

public class FashionAiResponse
{
    [JsonPropertyName("recommendation")]
    public string? Recommendation { get; set; }
    
    [JsonPropertyName("imagePrompt")]
    public string? ImagePrompt { get; set; }
}