using FluentValidation;
using HeimReport.Api.DTOs.Common;
using HeimReport.Api.DTOs.Positions;
using HeimReport.Api.Extensions;
using HeimReport.Api.Services.Positions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HeimReport.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class PositionsController(
    IPositionService positionService,
    IValidator<PositionCreateDto> createValidator,
    IValidator<PositionUpdateDto> updateValidator) : ControllerBase
{
    [HttpGet]
    [Authorize(Roles = "Admin,HR")]
    public async Task<ActionResult<PagedResultDto<PositionResponseDto>>> GetActivePaged(
        [FromQuery] PaginationQueryDto query, CancellationToken cancellationToken)
    {
        var result = await positionService.GetActivePagedAsync(query, cancellationToken);
        return Ok(result);
    }

    [HttpGet("inactive")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<PagedResultDto<PositionResponseDto>>> GetInactivePaged(
        [FromQuery] PaginationQueryDto query, CancellationToken cancellationToken)
    {
        var result = await positionService.GetInactivePagedAsync(query, cancellationToken);
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
        [FromBody] PositionCreateDto dto,
        CancellationToken cancellationToken)
    {
        await createValidator.ValidateOrThrowAsync(dto, cancellationToken);

        var result = await positionService.CreateAsync(dto, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [HttpPut("{id:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Update(
        int id,
        [FromBody] PositionUpdateDto dto,
        CancellationToken cancellationToken)
    {
        var context = new ValidationContext<PositionUpdateDto>(dto);
        context.RootContextData["PositionId"] = id;

        await updateValidator.ValidateOrThrowAsync(context, cancellationToken);

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

    [HttpPost("{id:int}/reactivate")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Reactivate(int id, CancellationToken cancellationToken)
    {
        await positionService.ReactivateAsync(id, cancellationToken);
        return NoContent();
    }

    [HttpPost("bulk-delete")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<BulkOperationResultDto>> DeleteMany(
        [FromBody] BulkIdsDto dto, CancellationToken cancellationToken)
    {
        var result = await positionService.DeleteManyAsync(dto, cancellationToken);
        return Ok(result);
    }

    [HttpPost("bulk-reactivate")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<BulkOperationResultDto>> ReactivateMany(
        [FromBody] BulkIdsDto dto, CancellationToken cancellationToken)
    {
        var result = await positionService.ReactivateManyAsync(dto, cancellationToken);
        return Ok(result);
    }
}