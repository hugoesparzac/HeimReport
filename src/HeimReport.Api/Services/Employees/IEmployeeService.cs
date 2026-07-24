using HeimReport.Api.DTOs.Employees;
using HeimReport.Api.Enums;

namespace HeimReport.Api.Services.Employees;

public interface IEmployeeService
{
    Task<(IEnumerable<EmployeeResponseDto> Items, int TotalCount)> GetAllAsync(
        EmployeeStatus? status,
        int? departmentId,
        int? positionId,
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken = default);

    Task<EmployeeResponseDto> GetByIdAsync(int id, CancellationToken cancellationToken = default);

    Task<EmployeeResponseDto> CreateAsync(EmployeeCreateDto dto, CancellationToken cancellationToken = default);

    Task UpdateAsync(int id, EmployeeUpdateDto dto, CancellationToken cancellationToken = default);

    Task DeleteAsync(int id, CancellationToken cancellationToken = default);
}