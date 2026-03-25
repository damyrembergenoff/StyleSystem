namespace StyleSystem.Shared.Dtos.Dashboard;

public class DashboardDto
{
    public int TotalRecommendations { get; set; }
    public int ThisMonth { get; set; }
    public IList<RecentActivityDto> RecentActivities { get; set; } = new List<RecentActivityDto>();
}

public class RecentActivityDto
{
    public string? Occasion { get; set; }
    public string? ImageUrl { get; set; }
}