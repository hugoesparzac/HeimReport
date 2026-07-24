using HeimReport.Api.DTOs.Countries;
using HeimReport.Api.Entities;

namespace HeimReport.Api.Mappers;

public static class CountryMapper
{
    public static CountryResponseDto ToResponseDto(this Country country)
    {
        return new CountryResponseDto
        {
            Id = country.Id,
            Name = country.Name,
            IsActive = country.IsActive
        };
    }

    public static Country ToEntity(this CountryCreateDto dto)
    {
        return new Country
        {
            Name = dto.Name,
            IsActive = dto.IsActive
        };
    }

    // Muta la entidad ya trackeada por EF; no crea una nueva instancia.
    public static void UpdateEntity(this CountryUpdateDto dto, Country country)
    {
        country.Name = dto.Name;
        country.IsActive = dto.IsActive;
    }
}