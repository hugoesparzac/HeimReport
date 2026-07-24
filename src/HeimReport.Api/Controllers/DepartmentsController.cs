using FluentValidation;
using HeimReport.Api.DTOs.Common;
using HeimReport.Api.DTOs.Departments;
using HeimReport.Api.Extensions;
using HeimReport.Api.Services.Departments;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HeimReport.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class DepartmentsController(
    IDepartmentService departmentService,
    IValidator<DepartmentCreateDto> createValidator,
    IValidator<DepartmentUpdateDto> updateValidator) : ControllerBase
{
    [HttpGet]
    [Authorize(Roles = "Admin,HR")]
    public async Task<ActionResult<PagedResultDto<DepartmentResponseDto>>> GetActivePaged(
        [FromQuery] PaginationQueryDto query, CancellationToken cancellationToken)
    {
        var result = await departmentService.GetActivePagedAsync(query, cancellationToken);
        return Ok(result);
    }

    [HttpGet("inactive")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<PagedResultDto<DepartmentResponseDto>>> GetInactivePaged(
        [FromQuery] PaginationQueryDto query, CancellationToken cancellationToken)
    {
        var result = await departmentService.GetInactivePagedAsync(query, cancellationToken);
        return Ok(result);
    }

    [HttpGet("{id:int}")]
    [Authorize(Roles = "Admin,HR")]
    public async Task<ActionResult<DepartmentResponseDto>> GetById(int id, CancellationToken cancellationToken)
    {
        var result = await departmentService.GetByIdAsync(id, cancellationToken);
        return Ok(result);
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<DepartmentResponseDto>> Create(
        [FromBody] DepartmentCreateDto dto,
        CancellationToken cancellationToken)
    {
        await createValidator.ValidateOrThrowAsync(dto, cancellationToken);

        var result = await departmentService.CreateAsync(dto, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [HttpPut("{id:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Update(
        int id,
        [FromBody] DepartmentUpdateDto dto,
        CancellationToken cancellationToken)
    {
        var context = new ValidationContext<DepartmentUpdateDto>(dto);
        context.RootContextData["DepartmentId"] = id;

        await updateValidator.ValidateOrThrowAsync(context, cancellationToken);

        await departmentService.UpdateAsync(id, dto, cancellationToken);
        return NoContent();
    }

    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        await departmentService.DeleteAsync(id, cancellationToken);
        return NoContent();
    }

    [HttpPost("{id:int}/reactivate")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Reactivate(int id, CancellationToken cancellationToken)
    {
        await departmentService.ReactivateAsync(id, cancellationToken);
        return NoContent();
    }

    [HttpPost("bulk-delete")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<BulkOperationResultDto>> DeleteMany(
        [FromBody] BulkIdsDto dto, CancellationToken cancellationToken)
    {
        var result = await departmentService.DeleteManyAsync(dto, cancellationToken);
        return Ok(result);
    }

    [HttpPost("bulk-reactivate")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<BulkOperationResultDto>> ReactivateMany(
        [FromBody] BulkIdsDto dto, CancellationToken cancellationToken)
    {
        var result = await departmentService.ReactivateManyAsync(dto, cancellationToken);
        return Ok(result);
    }
}