using Microsoft.EntityFrameworkCore;
using StyleSystem.Api.Abstractions;
using StyleSystem.Api.Data;
using StyleSystem.Shared.Dtos.Dashboard;

namespace StyleSystem.Api.Services;

public class DashboardService(
    StyleSystemDbContext dbContext,
    ILogger<DashboardService> logger) : IDashboardService
{
    public async Task<DashboardDto> GetDashboardAsync(Guid userId, CancellationToken cancellationToken)
    {
        logger.LogInformation("Fetching dashboard for user {UserId}", userId);

        var recommendations = await dbContext.Recommendations
            .Include(x => x.Images)
            .OrderByDescending(x => x.CreatedAt)
            .Where(r => r.UserId == userId)
            .Take(3)
            .ToListAsync(cancellationToken);

        var totalRecommendations = await dbContext.Recommendations.CountAsync(r => r.UserId == userId, cancellationToken);
        var thisMonthRecommendations = await dbContext.Recommendations.CountAsync(r => r.UserId == userId && r.CreatedAt >= DateTimeOffset.UtcNow.AddMonths(-1), cancellationToken);

        logger.LogInformation("User {UserId} has {Total} total recommendations, {ThisMonth} this month", userId, totalRecommendations, thisMonthRecommendations);
        logger.LogInformation("Recent activities for user {UserId}: {@Recommendations}", userId, recommendations.Select(r => new { r.Occasion, ImageUrl = r.Images.FirstOrDefault()?.ImageUrl }));
        return new DashboardDto
        {
            TotalRecommendations = totalRecommendations,
            ThisMonth = thisMonthRecommendations,
            RecentActivities = recommendations.Select(r => new RecentActivityDto
            {
                Occasion = r.Occasion,
                ImageUrl = r.Images.FirstOrDefault()?.ImageUrl,
            }).ToList()
        };
    }
}