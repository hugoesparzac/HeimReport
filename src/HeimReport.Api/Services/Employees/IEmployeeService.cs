using HeimReport.Api.DTOs.Common;
using HeimReport.Api.DTOs.Employees;

namespace HeimReport.Api.Services.Employees;

public interface IEmployeeService
{
    Task<PagedResultDto<EmployeeResponseDto>> GetPagedAsync(EmployeeQueryDto query, CancellationToken cancellationToken = default);
    Task<EmployeeResponseDto> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<EmployeeResponseDto> CreateAsync(EmployeeCreateDto dto, CancellationToken cancellationToken = default);
    Task UpdateAsync(int id, EmployeeUpdateDto dto, CancellationToken cancellationToken = default);
    Task<BulkOperationResultDto> TerminateManyAsync(EmployeeBulkTerminationDto dto, CancellationToken cancellationToken = default);
    Task<EmployeeResponseDto> UploadPhotoAsync(int id, IFormFile photo, CancellationToken cancellationToken = default);
    Task RemovePhotoAsync(int id, CancellationToken cancellationToken = default);
}