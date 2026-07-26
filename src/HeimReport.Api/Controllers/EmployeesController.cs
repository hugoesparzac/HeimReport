using FluentValidation;
using HeimReport.Api.DTOs.Common;
using HeimReport.Api.DTOs.Employees;
using HeimReport.Api.Extensions;
using HeimReport.Api.Services.Employees;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HeimReport.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class EmployeesController(
    IEmployeeService employeeService,
    IValidator<EmployeeCreateDto> createValidator,
    IValidator<EmployeeUpdateDto> updateValidator,
    IValidator<EmployeeBulkTerminationDto> terminationValidator,
    IValidator<UploadEmployeePhotoDto> uploadPhotoValidator) : ControllerBase
{
    [HttpGet]
    [Authorize(Roles = "Admin,HR")]
    public async Task<ActionResult<PagedResultDto<EmployeeResponseDto>>> GetPaged(
        [FromQuery] EmployeeQueryDto query, CancellationToken cancellationToken)
    {
        var result = await employeeService.GetPagedAsync(query, cancellationToken);
        return Ok(result);
    }

    [HttpGet("{id:int}")]
    [Authorize(Roles = "Admin,HR")]
    public async Task<ActionResult<EmployeeResponseDto>> GetById(int id, CancellationToken cancellationToken)
    {
        var result = await employeeService.GetByIdAsync(id, cancellationToken);
        return Ok(result);
    }

    [HttpPost]
    [Authorize(Roles = "Admin,HR")]
    public async Task<ActionResult<EmployeeResponseDto>> Create(
        [FromBody] EmployeeCreateDto dto,
        CancellationToken cancellationToken)
    {
        await createValidator.ValidateOrThrowAsync(dto, cancellationToken);

        var result = await employeeService.CreateAsync(dto, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [HttpPut("{id:int}")]
    [Authorize(Roles = "Admin,HR")]
    public async Task<IActionResult> Update(
        int id,
        [FromBody] EmployeeUpdateDto dto,
        CancellationToken cancellationToken)
    {
        var current = await employeeService.GetByIdAsync(id, cancellationToken);

        var snapshot = new EmployeeSnapshot(
            current.DepartmentId,
            current.PositionId,
            current.ManagerId,
            current.CurrentSalary,
            current.Status);

        var context = new ValidationContext<EmployeeUpdateDto>(dto);
        context.RootContextData["EmployeeId"] = id;
        context.RootContextData["CurrentSnapshot"] = snapshot;

        await updateValidator.ValidateOrThrowAsync(context, cancellationToken);

        await employeeService.UpdateAsync(id, dto, cancellationToken);
        return NoContent();
    }

    [HttpPost("bulk-terminate")]
    [Authorize(Roles = "Admin,HR")]
    public async Task<ActionResult<BulkOperationResultDto>> TerminateMany(
        [FromBody] EmployeeBulkTerminationDto dto,
        CancellationToken cancellationToken)
    {
        await terminationValidator.ValidateOrThrowAsync(dto, cancellationToken);

        var result = await employeeService.TerminateManyAsync(dto, cancellationToken);
        return Ok(result);
    }

    [HttpPost("{id:int}/photo")]
    [Authorize(Roles = "Admin,HR")]
    [RequestSizeLimit(5_000_000)]
    public async Task<ActionResult<EmployeeResponseDto>> UploadPhoto(
        int id, [FromForm] UploadEmployeePhotoDto dto, CancellationToken cancellationToken)
    {
        await uploadPhotoValidator.ValidateOrThrowAsync(dto, cancellationToken);

        var result = await employeeService.UploadPhotoAsync(id, dto.Photo, cancellationToken);
        return Ok(result);
    }

    [HttpDelete("{id:int}/photo")]
    [Authorize(Roles = "Admin,HR")]
    public async Task<IActionResult> RemovePhoto(int id, CancellationToken cancellationToken)
    {
        await employeeService.RemovePhotoAsync(id, cancellationToken);
        return NoContent();
    }
}