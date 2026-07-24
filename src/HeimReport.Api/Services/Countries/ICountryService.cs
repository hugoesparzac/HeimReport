using HeimReport.Api.DTOs.Common;
using HeimReport.Api.DTOs.Countries;

namespace HeimReport.Api.Services.Countries;

public interface ICountryService
{
    Task<PagedResultDto<CountryResponseDto>> GetActivePagedAsync(PaginationQueryDto query, CancellationToken cancellationToken = default);
    Task<PagedResultDto<CountryResponseDto>> GetInactivePagedAsync(PaginationQueryDto query, CancellationToken cancellationToken = default);
    Task<CountryResponseDto> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<CountryResponseDto> CreateAsync(CountryCreateDto dto, CancellationToken cancellationToken = default);
    Task UpdateAsync(int id, CountryUpdateDto dto, CancellationToken cancellationToken = default);
    Task DeleteAsync(int id, CancellationToken cancellationToken = default);
    Task ReactivateAsync(int id, CancellationToken cancellationToken = default);
    Task<BulkOperationResultDto> DeleteManyAsync(BulkIdsDto dto, CancellationToken cancellationToken = default);
    Task<BulkOperationResultDto> ReactivateManyAsync(BulkIdsDto dto, CancellationToken cancellationToken = default);
}