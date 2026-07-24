using FluentValidation;
using HeimReport.Api.DTOs.Common;
using HeimReport.Api.DTOs.Countries;
using HeimReport.Api.Extensions;
using HeimReport.Api.Services.Countries;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HeimReport.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class CountriesController(
    ICountryService countryService,
    IValidator<CountryCreateDto> createValidator,
    IValidator<CountryUpdateDto> updateValidator) : ControllerBase
{
    [HttpGet]
    [Authorize(Roles = "Admin,HR")]
    public async Task<ActionResult<PagedResultDto<CountryResponseDto>>> GetActivePaged(
        [FromQuery] PaginationQueryDto query, CancellationToken cancellationToken)
    {
        var result = await countryService.GetActivePagedAsync(query, cancellationToken);
        return Ok(result);
    }

    [HttpGet("inactive")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<PagedResultDto<CountryResponseDto>>> GetInactivePaged(
        [FromQuery] PaginationQueryDto query, CancellationToken cancellationToken)
    {
        var result = await countryService.GetInactivePagedAsync(query, cancellationToken);
        return Ok(result);
    }

    [HttpGet("{id:int}")]
    [Authorize(Roles = "Admin,HR")]
    public async Task<ActionResult<CountryResponseDto>> GetById(int id, CancellationToken cancellationToken)
    {
        var result = await countryService.GetByIdAsync(id, cancellationToken);
        return Ok(result);
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<CountryResponseDto>> Create(
        [FromBody] CountryCreateDto dto,
        CancellationToken cancellationToken)
    {
        await createValidator.ValidateOrThrowAsync(dto, cancellationToken);

        var result = await countryService.CreateAsync(dto, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [HttpPut("{id:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Update(
        int id,
        [FromBody] CountryUpdateDto dto,
        CancellationToken cancellationToken)
    {
        var context = new ValidationContext<CountryUpdateDto>(dto);
        context.RootContextData["CountryId"] = id;

        await updateValidator.ValidateOrThrowAsync(context, cancellationToken);

        await countryService.UpdateAsync(id, dto, cancellationToken);
        return NoContent();
    }

    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        await countryService.DeleteAsync(id, cancellationToken);
        return NoContent();
    }

    [HttpPost("{id:int}/reactivate")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Reactivate(int id, CancellationToken cancellationToken)
    {
        await countryService.ReactivateAsync(id, cancellationToken);
        return NoContent();
    }

    [HttpPost("bulk-delete")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<BulkOperationResultDto>> DeleteMany(
        [FromBody] BulkIdsDto dto, CancellationToken cancellationToken)
    {
        var result = await countryService.DeleteManyAsync(dto, cancellationToken);
        return Ok(result);
    }

    [HttpPost("bulk-reactivate")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<BulkOperationResultDto>> ReactivateMany(
        [FromBody] BulkIdsDto dto, CancellationToken cancellationToken)
    {
        var result = await countryService.ReactivateManyAsync(dto, cancellationToken);
        return Ok(result);
    }
}