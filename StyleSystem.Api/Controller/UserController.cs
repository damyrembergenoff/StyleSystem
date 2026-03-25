using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StyleSystem.Api.Abstractions;
using StyleSystem.Shared.Dtos;
using StyleSystem.Web.Dtos.CompleteProfile;

namespace StyleSystem.Api.Controller;

[ApiController, Route("api/[controller]")]
public class UserController(
    IUserService service,
    IAuthenticationStateService authenticationStateService,
    ILogger<UserController> logger) : ControllerBase
{
    [HttpPost("register")]
    public async Task<IActionResult> RegisterUserAsync(RegisterUserDto user, CancellationToken cancellationToken = default)
    {
        var loginResponse = await service.RegisterAsync(user, cancellationToken);
        
        if (string.IsNullOrEmpty(loginResponse.Token))
            return BadRequest("Username already exists");

        return Ok(loginResponse);
    }

    [HttpPost("login")]
    public async Task<IActionResult> LoginAsync(LoginUserDto user, CancellationToken cancellationToken = default)
    {
        var loginResponse = await service.LoginAsync(user, cancellationToken);

        if (string.IsNullOrEmpty(loginResponse.Token))
            return BadRequest("Invalid username or password");

        return Ok(loginResponse);
    }

    [Authorize]
    [HttpGet("profile")]
    public async Task<IActionResult> GetProfileAsync(CancellationToken cancellationToken = default)
    {
        var user = await service.GetUserAsync(authenticationStateService.UserId, cancellationToken);

        if (user is null)
            return Unauthorized();

        return Ok(user);
    }

    [Authorize]
    [HttpPut("profile")]
    public async Task<IActionResult> UpdateProfileAsync(ProfileModel profile, CancellationToken cancellationToken = default)
    {
        try
        {
            logger.LogInformation("UpdateProfileAsync called for UserId: {UserId}", authenticationStateService.UserId);
            logger.LogInformation("Profile data received - FullName: {FullName}, Gender: {Gender}, BodyType: {BodyType}, Height: {Height}, Weight: {Weight}, SkinTone: {SkinTone}",
                profile?.FullName, profile?.Gender, profile?.BodyType, profile?.Height, profile?.Weight, profile?.SkinTone);

            await service.UpdateUserAsync(authenticationStateService.UserId, profile!, cancellationToken);

            logger.LogInformation("Profile updated successfully for UserId: {UserId}", authenticationStateService.UserId);
            return Ok();
        }
        catch (InvalidOperationException ex)
        {
            logger.LogWarning("InvalidOperationException in UpdateProfileAsync: {Message}", ex.Message);
            return Unauthorized(ex.Message);
        }
        catch (ArgumentOutOfRangeException ex)
        {
            logger.LogWarning("ArgumentOutOfRangeException in UpdateProfileAsync: {Message}. Invalid field: {FieldName}", ex.Message, ex.ParamName);
            return BadRequest($"Invalid value: {ex.ParamName}. {ex.Message}");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unexpected exception in UpdateProfileAsync for UserId: {UserId}", authenticationStateService.UserId);
            return StatusCode(500, "An error occurred while updating the profile.");
        }
    }

    [Authorize]
    [HttpDelete("me")]
    public async Task<IActionResult> DeleteAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            await service.DeleteUserAsync(authenticationStateService.UserId, cancellationToken);
            return Ok();
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(ex.Message);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unexpected exception in DeleteAsync for UserId: {UserId}", authenticationStateService.UserId);
            return StatusCode(500, "An error occurred while deleting the account.");
        }
    }

    [Authorize]
    [HttpPut("change-password")]
    public async Task<IActionResult> ChangePasswordAsync(ChangePasswordDto changePassword, CancellationToken cancellationToken = default)
    {
        try
        {
            logger.LogInformation("ChangePasswordAsync called for UserId: {UserId}", authenticationStateService.UserId);

            await service.ChangePasswordAsync(authenticationStateService.UserId, changePassword, cancellationToken);

            logger.LogInformation("Password changed successfully for UserId: {UserId}", authenticationStateService.UserId);
            return Ok();
        }
        catch (InvalidOperationException ex)
        {
            logger.LogWarning("InvalidOperationException in ChangePasswordAsync: {Message}", ex.Message);
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unexpected exception in ChangePasswordAsync for UserId: {UserId}", authenticationStateService.UserId);
            return StatusCode(500, "An error occurred while changing the password.");
        }
    }
}