using System.Net.Http.Json;
using StyleSystem.Shared.Dtos.Dashboard;

namespace StyleSystem.Web.Services;

public class DashboardService(IHttpClientFactory httpClientFactory)
{
    private readonly string key = "api/dashboards";
    private readonly HttpClient privateHttp = httpClientFactory.CreateClient("Private");

    public async Task<DashboardDto?> GetAsync(CancellationToken cancellationToken = default)
    {
        var dashboard = await privateHttp.GetFromJsonAsync<DashboardDto?>(key, cancellationToken);
        return dashboard;
    }
}