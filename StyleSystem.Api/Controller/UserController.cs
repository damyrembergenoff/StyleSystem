using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StyleSystem.Api.Abstractions;
using StyleSystem.Shared.Dtos;

namespace StyleSystem.Api.Controller;

[ApiController, Route("api/[controller]")]
public class UserController(
    IUserService service,
    IAuthenticationStateService authenticationStateService) : ControllerBase
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
    [HttpGet("me")]
    public async Task<IActionResult> Me(CancellationToken cancellationToken = default)
    {
        var user = await service.GetUserAsync(authenticationStateService.UserId, cancellationToken);

        if (user is null)
            return Unauthorized();

        return Ok(user);
    }
}