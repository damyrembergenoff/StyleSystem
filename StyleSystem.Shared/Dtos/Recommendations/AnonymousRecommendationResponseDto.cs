namespace StyleSystem.Web.Dtos.Recommendations;

public class AnonymousRecommendationResponseDto
{
    public string? RecommendationText { get; set; }
    public List<string> ImageUrls { get; set; } = [];
}