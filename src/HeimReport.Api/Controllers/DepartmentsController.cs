using HeimReport.Api.DTOs.Departments;
using HeimReport.Api.Services.Departments;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HeimReport.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class DepartmentsController(IDepartmentService departmentService) : ControllerBase
{
    [HttpGet]
    [Authorize(Roles = "Admin,HR")]
    public async Task<ActionResult<IEnumerable<DepartmentResponseDto>>> GetAll(CancellationToken cancellationToken)
    {
        var result = await departmentService.GetAllAsync(cancellationToken);
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
        [FromBody] DepartmentCreateUpdateDto dto,
        CancellationToken cancellationToken)
    {
        var result = await departmentService.CreateAsync(dto, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [HttpPut("{id:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Update(
        int id,
        [FromBody] DepartmentCreateUpdateDto dto,
        CancellationToken cancellationToken)
    {
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
}