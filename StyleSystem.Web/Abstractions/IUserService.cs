using StyleSystem.Shared.Dtos;

namespace StyleSystem.Web.Abstractions;

public interface IUserService
{
    ValueTask<bool> RegisterAsync(RegisterUserDto user, CancellationToken cancellationToken = default);
    ValueTask<bool> LoginAsync(LoginUserDto user, CancellationToken cancellationToken = default);
}