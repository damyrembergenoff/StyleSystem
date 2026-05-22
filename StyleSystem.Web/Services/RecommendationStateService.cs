namespace StyleSystem.Web.Services;

public class RecommendationStateService
{
    public string? RecommendationText { get; private set; }
    public bool IsTranslated { get; private set; } = true;
    public List<string> ImageUrls { get; private set; } = [];

    public bool HasResult => !string.IsNullOrEmpty(RecommendationText) || ImageUrls.Count > 0;

    public void SetResult(string? recommendationText, bool isTranslated, List<string> imageUrls)
    {
        RecommendationText = recommendationText;
        IsTranslated = isTranslated;
        ImageUrls = imageUrls ?? [];
    }

    public void Clear()
    {
        RecommendationText = null;
        IsTranslated = true;
        ImageUrls = [];
    }
}
