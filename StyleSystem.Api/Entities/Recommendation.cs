namespace StyleSystem.Api.Entities;

public class Recommendation : EntityBase
{
    public string? Occasion { get; set; }
    public string? Season { get; set; }
    public string? Temperature { get; set; }
    public string? AdditionalPreferences { get; set; }
    public string? RecommendationText { get; set; }

    public User? User { get; set; }
    public Guid UserId { get; set; }
    
    public IList<RecommendationImage> Images { get; set; } = [];
}
