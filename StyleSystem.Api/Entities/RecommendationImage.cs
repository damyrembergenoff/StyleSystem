namespace StyleSystem.Api.Entities;

public class RecommendationImage : EntityBase
{
    public string? ImageUrl { get; set; }
    public int Order { get; set; }

    public Guid RecommendationId { get; set; }
    public Recommendation? Recommendation { get; set; }
}