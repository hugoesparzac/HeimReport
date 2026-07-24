using HeimReport.Api.DTOs.Employees;
using HeimReport.Api.Enums;
using HeimReport.Api.Services.Employees;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HeimReport.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class EmployeesController(IEmployeeService employeeService) : ControllerBase
{
    [HttpGet]
    [Authorize(Roles = "Admin,HR")]
    public async Task<ActionResult> GetAll(
        [FromQuery] EmployeeStatus? status,
        [FromQuery] int? departmentId,
        [FromQuery] int? positionId,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        var (items, totalCount) = await employeeService.GetAllAsync(
            status, departmentId, positionId, pageNumber, pageSize, cancellationToken);

        return Ok(new { items, totalCount });
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
        await employeeService.UpdateAsync(id, dto, cancellationToken);
        return NoContent();
    }

    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        await employeeService.DeleteAsync(id, cancellationToken);
        return NoContent();
    }
}