using StyleSystem.Shared.Dtos;

namespace StyleSystem.Api.Abstractions;

public interface IUserService
{
    ValueTask<LoginResponse> RegisterAsync(RegisterUserDto user, CancellationToken cancellationToken = default);
    ValueTask<UserDto> GetUserAsync(Guid userId, CancellationToken cancellationToken = default);
    ValueTask<LoginResponse> LoginAsync(LoginUserDto user, CancellationToken cancellationToken = default);
}