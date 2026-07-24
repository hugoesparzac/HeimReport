using HeimReport.Api.DTOs.Positions;
using HeimReport.Api.Services.Positions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HeimReport.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class PositionsController(IPositionService positionService) : ControllerBase
{
    [HttpGet]
    [Authorize(Roles = "Admin,HR")]
    public async Task<ActionResult<IEnumerable<PositionResponseDto>>> GetAll(CancellationToken cancellationToken)
    {
        var result = await positionService.GetAllAsync(cancellationToken);
        return Ok(result);
    }

    [HttpGet("{id:int}")]
    [Authorize(Roles = "Admin,HR")]
    public async Task<ActionResult<PositionResponseDto>> GetById(int id, CancellationToken cancellationToken)
    {
        var result = await positionService.GetByIdAsync(id, cancellationToken);
        return Ok(result);
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<PositionResponseDto>> Create(
        [FromBody] PositionCreateUpdateDto dto,
        CancellationToken cancellationToken)
    {
        var result = await positionService.CreateAsync(dto, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [HttpPut("{id:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Update(
        int id,
        [FromBody] PositionCreateUpdateDto dto,
        CancellationToken cancellationToken)
    {
        await positionService.UpdateAsync(id, dto, cancellationToken);
        return NoContent();
    }

    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        await positionService.DeleteAsync(id, cancellationToken);
        return NoContent();
    }
}