using System.Security.Claims;
using StyleSystem.Api.Abstractions;
using StyleSystem.Api.Entities;

namespace StyleSystem.Api.Services;

public class AuthenticationStateService(
    IHttpContextAccessor httpContextAccessor) : IAuthenticationStateService
{
    private ClaimsPrincipal? user => httpContextAccessor?.HttpContext?.User;

    public User? User
    {
        get
        {
            if (IsAuthenticated is false || string.IsNullOrWhiteSpace(UserId.ToString()))
                return null;
            
            User user = new();

            user.Id = UserId;
            user.FullName = FullName;
            user.Username = Username;

            return user;
        }
    }

    public bool IsAuthenticated => user?.Identity?.IsAuthenticated is true;

    public Guid UserId
    {
        get
        {
            var userIdClaim = user?
                .Claims
                .FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)
                ?.Value;

        if (Guid.TryParse(userIdClaim, out var userId))
            return userId;

        throw new UnauthorizedAccessException("UserId claim not found or invalid.");
        }
    }

    public string? FullName => (user?.Claims?.FirstOrDefault(c => c.Type.Equals("FullName")))?.Value;

    public string? Username => (user?.Claims?.FirstOrDefault(c => c.Type.Equals("username")))?.Value;
}