namespace HeimReport.Api.DTOs.Countries;

public record CountryResponseDto
{
    public int Id { get; init; }
    public required string Name { get; init; }
}