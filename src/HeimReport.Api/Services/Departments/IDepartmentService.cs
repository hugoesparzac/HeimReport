using HeimReport.Api.DTOs.Departments;

namespace HeimReport.Api.Services.Departments;

public interface IDepartmentService
{
    Task<IEnumerable<DepartmentResponseDto>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<DepartmentResponseDto> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<DepartmentResponseDto> CreateAsync(DepartmentCreateUpdateDto dto, CancellationToken cancellationToken = default);
    Task UpdateAsync(int id, DepartmentCreateUpdateDto dto, CancellationToken cancellationToken = default);
    Task DeleteAsync(int id, CancellationToken cancellationToken = default);
}