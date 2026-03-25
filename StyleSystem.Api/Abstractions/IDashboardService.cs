using StyleSystem.Shared.Dtos.Dashboard;

namespace StyleSystem.Api.Abstractions;

public interface IDashboardService
{
    public Task<DashboardDto> GetDashboardAsync(Guid userId, CancellationToken cancellationToken);
}