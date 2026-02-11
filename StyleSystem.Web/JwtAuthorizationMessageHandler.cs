using System.Net;
using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components;

namespace StyleSystem.Web;

public class JwtAuthorizationMessageHandler(
        ILogger<JwtAuthorizationMessageHandler> logger,
        ILocalStorageService localStorage,
        NavigationManager navigationManager): DelegatingHandler
{
    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken = default)
    {
        try
        {
            var token = await localStorage.GetItemAsync<string>("authToken", cancellationToken);

            if(string.IsNullOrWhiteSpace(token) is false)
            {
                request.Headers.Authorization = 
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            }

            var response = await base.SendAsync(request, cancellationToken);

            if(response.StatusCode == HttpStatusCode.Unauthorized)
            {
                logger.LogInformation("Jwt invalid or expired. Redirecting to Login page.");

                await localStorage.RemoveItemAsync("authToken");
                navigationManager.NavigateTo("/login");
            }

            return response;
        }
        catch(HttpRequestException ex)
        {
            logger.LogError(ex, "Network error while calling Api");
            throw;
        }
    }
}