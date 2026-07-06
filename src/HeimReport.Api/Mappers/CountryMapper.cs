using HeimReport.Api.DTOs.Countries;
using HeimReport.Api.Entities;

namespace HeimReport.Api.Mappers;

public static class CountryMapper
{
    public static CountryResponseDto ToResponseDto(this Country country)
    {
        return new CountryResponseDto(country.Id,  country.Name);
    }

    public static Country ToEntity(this CountryCreateUpdateDto dto)
    {
        return new Country
        {
            Name = dto.Name,
        };
    }
}