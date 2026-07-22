namespace HeimReport.Api.DTOs.Countries;

public record CountryCreateUpdateDto
{
    public required string Name { get; init; }
    public bool IsActive { get; init; } = true;
}