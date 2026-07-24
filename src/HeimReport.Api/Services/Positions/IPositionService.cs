using HeimReport.Api.DTOs.Common;
using HeimReport.Api.DTOs.Positions;

namespace HeimReport.Api.Services.Positions;

public interface IPositionService
{
    Task<PagedResultDto<PositionResponseDto>> GetActivePagedAsync(PaginationQueryDto query, CancellationToken cancellationToken = default);
    Task<PagedResultDto<PositionResponseDto>> GetInactivePagedAsync(PaginationQueryDto query, CancellationToken cancellationToken = default);
    Task<PositionResponseDto> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<PositionResponseDto> CreateAsync(PositionCreateDto dto, CancellationToken cancellationToken = default);
    Task UpdateAsync(int id, PositionUpdateDto dto, CancellationToken cancellationToken = default);
    Task DeleteAsync(int id, CancellationToken cancellationToken = default);
    Task ReactivateAsync(int id, CancellationToken cancellationToken = default);
    Task<BulkOperationResultDto> DeleteManyAsync(BulkIdsDto dto, CancellationToken cancellationToken = default);
    Task<BulkOperationResultDto> ReactivateManyAsync(BulkIdsDto dto, CancellationToken cancellationToken = default);
}