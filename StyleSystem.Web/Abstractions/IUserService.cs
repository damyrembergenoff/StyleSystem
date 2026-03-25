using StyleSystem.Shared.Dtos;
using StyleSystem.Web.Dtos.CompleteProfile;

namespace StyleSystem.Web.Abstractions;

public interface IUserService
{
    ValueTask<bool> RegisterAsync(RegisterUserDto user, CancellationToken cancellationToken = default);
    ValueTask<bool> LoginAsync(LoginUserDto user, CancellationToken cancellationToken = default);
    ValueTask<bool> UpdateUserAsync(ProfileModel profile, CancellationToken cancellationToken = default);
}