using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components.Authorization;

namespace StyleSystem.Web.Auth;

public class CustomAuthStateProvider(ILocalStorageService localStorageService) : AuthenticationStateProvider
{
    private readonly ClaimsPrincipal anonymous = new(new ClaimsIdentity());

    public override async Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        var token = await localStorageService.GetItemAsync<string>("authToken");
        Console.WriteLine($"Retrieved jwt token: {token}");

        if (string.IsNullOrWhiteSpace(token))
        {
            return new AuthenticationState(anonymous);
        }

        var handler = new JwtSecurityTokenHandler();
        var jwtToken = handler.ReadJwtToken(token);

        if(jwtToken.ValidTo < DateTime.UtcNow)
        {
            await localStorageService.RemoveItemAsync("authToken");
            return new AuthenticationState(anonymous);
        }

        var identity = new ClaimsIdentity(jwtToken.Claims, "jwt");
        var user = new ClaimsPrincipal(identity);

        return new AuthenticationState(user);
    }

    public void NotifyUserAuthentication(string token)
    {
        var handler = new JwtSecurityTokenHandler();
        var jwt = handler.ReadJwtToken(token);

        var identity = new ClaimsIdentity(jwt.Claims, "jwt");
        var user = new ClaimsPrincipal(identity);

        NotifyAuthenticationStateChanged(
            Task.FromResult(new AuthenticationState(user)));
    }

    public void NotifyUserLogout()
    {
        NotifyAuthenticationStateChanged(
            Task.FromResult(new AuthenticationState(anonymous)));
    }
}