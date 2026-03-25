namespace StyleSystem.Api.Dtos.Recommendations;

public class AnonymousRecommendationDto
{
    public string Gender { get; set; } = string.Empty;
    public string BodyType { get; set; } = string.Empty;
    public string AgeRange { get; set; } = string.Empty;
    public string Occasion { get; set; } = string.Empty;
    public string Season { get; set; } = string.Empty;
    public string Temperature { get; set; } = string.Empty;
    public string? AdditionalPreferences { get; set; }
}