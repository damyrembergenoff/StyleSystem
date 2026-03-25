using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StyleSystem.Api.Abstractions;
using StyleSystem.Api.Dtos.Recommendations;
using StyleSystem.Shared.Dtos.Recommendations;
using StyleSystem.Shared.DTOs.Recommendations;

namespace StyleSystem.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class RecommendationsController(
    IRecommendationService recommendationService,
    IAuthenticationStateService authStateService
) : ControllerBase
{
    [HttpPost]
    [Authorize]
    public async Task<ActionResult<RecommendationResponseDto>> Create(
        [FromBody] CreateRecommendationDto dto,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var userId = authStateService.UserId;
            
            var result = await recommendationService.CreateAsync(dto, userId, cancellationToken);
            
            return Ok(result);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPost("anonymous")]
    [AllowAnonymous]
    public async Task<ActionResult<AnonymousRecommendationResponseDto>> CreateAnonymous(
        [FromBody] AnonymousRecommendationDto dto,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var result = await recommendationService.CreateAnonymousAsync(dto, cancellationToken);
            
            return Ok(result);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpGet("history")]
    [Authorize]
    public async Task<ActionResult<IList<RecommendationResponseDto>>> GetHistory(CancellationToken cancellationToken = default)
    {
        try
        {
            var userId = authStateService.UserId;
            
            var result = await recommendationService.GetUserHistoryAsync(userId, cancellationToken);
            
            return Ok(result);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpGet("{id:guid}")]
    [Authorize]
    public async Task<ActionResult<RecommendationResponseDto>> GetById(Guid id, CancellationToken cancellationToken = default)
    {
        try
        {
            var userId = authStateService.UserId;
            
            var result = await recommendationService.GetByIdAsync(id, userId, cancellationToken);
            
            return Ok(result);
        }
        catch (Exception ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

}