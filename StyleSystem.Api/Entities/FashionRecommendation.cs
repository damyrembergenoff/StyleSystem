namespace StyleSystem.Api.Entities;

public class FashionRecommendation : EntityBase
{
    public string? TextRecommendation { get; set; }
    public string? ImageUrl { get; set; }
    public string? ImagePrompt { get; set; }

    public User? User { get; set; }
    public Guid UserId { get; set; }
}