using StyleSystem.Shared.Dtos;
using StyleSystem.Web.Dtos.CompleteProfile;

namespace StyleSystem.Api.Abstractions;

public interface IUserService
{
    ValueTask<LoginResponse> RegisterAsync(RegisterUserDto user, CancellationToken cancellationToken = default);
    ValueTask<UserDto> GetUserAsync(Guid userId, CancellationToken cancellationToken = default);
    ValueTask<LoginResponse> LoginAsync(LoginUserDto user, CancellationToken cancellationToken = default);
    ValueTask UpdateUserAsync(Guid userId, ProfileModel model, CancellationToken cancellationToken = default);
    ValueTask DeleteUserAsync(Guid userId, CancellationToken cancellationToken = default);
    ValueTask ChangePasswordAsync(Guid userId, ChangePasswordDto changePassword, CancellationToken cancellationToken = default);
}