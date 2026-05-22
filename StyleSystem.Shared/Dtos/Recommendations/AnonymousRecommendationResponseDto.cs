namespace StyleSystem.Web.Dtos.Recommendations;

public class AnonymousRecommendationResponseDto
{
    public string? RecommendationText { get; set; }
    public bool IsTranslated { get; set; } = true;
    public List<string> ImageUrls { get; set; } = [];
}