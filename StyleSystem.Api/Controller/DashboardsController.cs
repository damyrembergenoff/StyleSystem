using Microsoft.AspNetCore.Mvc;
using StyleSystem.Api.Abstractions;

namespace StyleSystem.Api.Controller;

[ApiController, Route("api/[controller]")]
public class DashboardsController(
    IDashboardService dashboardService,
    IAuthenticationStateService authStateService) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetDashboardAsync(CancellationToken cancellationToken)
    {
        var userId = authStateService.UserId;
        var dashboard = await dashboardService.GetDashboardAsync(userId, cancellationToken);
        return Ok(dashboard);
    }
}