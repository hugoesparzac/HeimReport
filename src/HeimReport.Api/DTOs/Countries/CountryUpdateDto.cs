namespace HeimReport.Api.DTOs.Countries;

public record CountryUpdateDto
{
    public required int Id { get; init; }
    public required string Name { get; init; }
    public bool IsActive { get; init; }
}