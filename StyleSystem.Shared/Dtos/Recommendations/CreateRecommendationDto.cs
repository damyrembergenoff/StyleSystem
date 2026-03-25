namespace StyleSystem.Shared.Dtos.Recommendations;

public class CreateRecommendationDto
{
    public string Occasion { get; set; } = string.Empty;
    public string Season { get; set; } = string.Empty;
    public string Temperature { get; set; } = string.Empty;
    public string? AdditionalPreferences { get; set; }
}