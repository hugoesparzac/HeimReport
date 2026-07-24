using HeimReport.Api.DTOs.Countries;

namespace HeimReport.Api.Services.Countries;

public interface ICountryService
{
    Task<IEnumerable<CountryResponseDto>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<CountryResponseDto> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<CountryResponseDto> CreateAsync(CountryCreateUpdateDto dto, CancellationToken cancellationToken = default);
    Task UpdateAsync(int id, CountryCreateUpdateDto dto, CancellationToken cancellationToken = default);
    Task DeleteAsync(int id, CancellationToken cancellationToken = default);
}