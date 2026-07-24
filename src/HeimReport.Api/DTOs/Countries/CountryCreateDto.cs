namespace HeimReport.Api.DTOs.Countries;

public record CountryCreateDto
{
    public required string Name { get; init; }
    public bool IsActive { get; init; } = true;
}