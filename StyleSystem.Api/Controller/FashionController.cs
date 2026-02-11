using Microsoft.AspNetCore.Mvc;
using StyleSystem.Api.Abstractions;
using StyleSystem.Shared.Dtos;

namespace StyleSystem.Api.Controller;

[ApiController, Route("api/[controller]")]
public class FashionController(IGroqService groqService) : ControllerBase
{
    [HttpPost("recommend")]
    public async Task<IActionResult> Recommend([FromBody] FashionRequest fashionRequest, CancellationToken cancellationToken = default)
    {
        if(string.IsNullOrWhiteSpace(fashionRequest.Prompt))
            return BadRequest("Prompt is required");
        
        var recommendation = await groqService.GetFashionRecommmendationsAsync(fashionRequest.Prompt, cancellationToken);

        return Ok(new FashionRecommendationResponse
        {
            Recommendation = recommendation
        });
    }
}