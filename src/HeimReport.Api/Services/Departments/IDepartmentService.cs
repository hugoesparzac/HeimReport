using HeimReport.Api.DTOs.Common;
using HeimReport.Api.DTOs.Departments;

namespace HeimReport.Api.Services.Departments;

public interface IDepartmentService
{
    Task<PagedResultDto<DepartmentResponseDto>> GetActivePagedAsync(PaginationQueryDto query, CancellationToken cancellationToken = default);
    Task<PagedResultDto<DepartmentResponseDto>> GetInactivePagedAsync(PaginationQueryDto query, CancellationToken cancellationToken = default);
    Task<DepartmentResponseDto> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<DepartmentResponseDto> CreateAsync(DepartmentCreateDto dto, CancellationToken cancellationToken = default);
    Task UpdateAsync(int id, DepartmentUpdateDto dto, CancellationToken cancellationToken = default);
    Task DeleteAsync(int id, CancellationToken cancellationToken = default);
    Task ReactivateAsync(int id, CancellationToken cancellationToken = default);
    Task<BulkOperationResultDto> DeleteManyAsync(BulkIdsDto dto, CancellationToken cancellationToken = default);
    Task<BulkOperationResultDto> ReactivateManyAsync(BulkIdsDto dto, CancellationToken cancellationToken = default);
}