using StyleSystem.Api.Entities;

namespace StyleSystem.Api.Abstractions;

public interface IAuthenticationStateService
{
    User? User { get; }
    bool IsAuthenticated { get; }
    Guid UserId { get; }
    string? FullName { get; }
    string? Username { get; }
}