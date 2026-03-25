using System.Net.Http.Json;
using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components.Authorization;
using StyleSystem.Shared.Dtos;
using StyleSystem.Web.Abstractions;
using StyleSystem.Web.Auth;
using StyleSystem.Web.Dtos.CompleteProfile;

namespace StyleSystem.Web.Services;

public class UserService(
    ILocalStorageService localStorageService,
    AuthenticationStateProvider authProvider,
    IHttpClientFactory httpClientFactory) : IUserService
{
    private string key = "api/user/";
    private HttpClient publicHttp = httpClientFactory.CreateClient("Public");
    private HttpClient privateHttp = httpClientFactory.CreateClient("Private");

    public async ValueTask<bool> LoginAsync(LoginUserDto user, CancellationToken cancellationToken = default)
    {
        try
        {
            var result = await publicHttp.PostAsJsonAsync(key + "login", user, cancellationToken);

            if(result.IsSuccessStatusCode)
            {
                var response = await result.Content.ReadFromJsonAsync<LoginResponse>(cancellationToken: cancellationToken);
                await localStorageService.SetItemAsync("authToken", response?.Token ?? string.Empty, cancellationToken);
                
                var customAuthProvider = (CustomAuthStateProvider)authProvider;
                customAuthProvider.NotifyUserAuthentication(response!.Token!);
                
                return true;
            }
            
            return false;
        }
        catch(Exception)
        {
            return false;
        }
    }

    public async ValueTask<bool> RegisterAsync(RegisterUserDto user, CancellationToken cancellationToken = default)
    {
        try
        {
            var result = await publicHttp.PostAsJsonAsync(key + "register", user, cancellationToken);

            if(result.IsSuccessStatusCode)
            {
                var response = await result.Content.ReadFromJsonAsync<LoginResponse>(cancellationToken: cancellationToken);
                await localStorageService.SetItemAsync("authToken", response?.Token ?? string.Empty, cancellationToken);
                
                var customAuthProvider = (CustomAuthStateProvider)authProvider;
                customAuthProvider.NotifyUserAuthentication(response!.Token!);
                
                return true;
            }

            return false;
        }
        catch(Exception)
        {
            return false;
        }
    }

    public async ValueTask<bool> UpdateUserAsync(ProfileModel profile, CancellationToken cancellationToken = default)
    {
        try
        {
            var result = await privateHttp.PutAsJsonAsync(key + "profile", profile, cancellationToken);

            return result.IsSuccessStatusCode;
        }
        catch(Exception)
        {
            return false;
        }
    }
}