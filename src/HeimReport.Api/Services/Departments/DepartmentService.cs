using HeimReport.Api.DTOs.Departments;
using HeimReport.Api.Exceptions;
using HeimReport.Api.Mappers;
using HeimReport.Api.Repositories.Departments;
using Microsoft.EntityFrameworkCore;

namespace HeimReport.Api.Services.Departments;

public class DepartmentService(IDepartmentRepository departmentRepository) : IDepartmentService
{
    public async Task<IEnumerable<DepartmentResponseDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var departments = await departmentRepository.Query().ToListAsync(cancellationToken);
        return departments.Select(d => d.ToResponseDto());
    }

    public async Task<DepartmentResponseDto> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var department = await departmentRepository.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException($"Department with ID {id} was not found.");

        return department.ToResponseDto();
    }

    public async Task<DepartmentResponseDto> CreateAsync(DepartmentCreateUpdateDto dto, CancellationToken cancellationToken = default)
    {
        if (await departmentRepository.ExistsByNameAsync(dto.Name, cancellationToken))
            throw DomainException.AlreadyExists("Department", "Name", dto.Name);

        var department = dto.ToEntity();

        await departmentRepository.AddAsync(department, cancellationToken);
        await departmentRepository.SaveChangesAsync(cancellationToken);

        return department.ToResponseDto();
    }

    public async Task UpdateAsync(int id, DepartmentCreateUpdateDto dto, CancellationToken cancellationToken = default)
    {
        var department = await departmentRepository.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException($"Department with ID {id} was not found.");

        if (!string.Equals(department.Name, dto.Name, StringComparison.OrdinalIgnoreCase) &&
            await departmentRepository.ExistsByNameAsync(dto.Name, cancellationToken))
        {
            throw DomainException.AlreadyExists("Department", "Name", dto.Name);
        }

        department.Name = dto.Name;

        departmentRepository.Update(department);
        await departmentRepository.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var department = await departmentRepository.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException($"Department with ID {id} was not found.");

        if (await departmentRepository.IsReferencedByEmployeeAsync(id, cancellationToken))
        {
            throw DomainException.EntityInUse("Department", "it is currently assigned to one or more employees");
        }

        departmentRepository.Remove(department);
        await departmentRepository.SaveChangesAsync(cancellationToken);
    }
}