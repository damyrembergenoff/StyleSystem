namespace StyleSystem.Shared.DTOs.Recommendations;

public class RecommendationResponseDto
{
    public Guid Id { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public string? Occasion { get; set; }
    public string? Season { get; set; }
    public string? Temperature { get; set; }
    public string? AdditionalPreferences { get; set; }
    public string? RecommendationText { get; set; }
    public List<RecommendationImageDto> Images { get; set; } = [];
}

public class RecommendationImageDto
{
    public Guid Id { get; set; }
    public string? ImageUrl { get; set; }
    public int Order { get; set; }
}