using HeimReport.Api.DTOs.Employees;
using HeimReport.Api.Entities;
using HeimReport.Api.Enums;
using HeimReport.Api.Exceptions;
using HeimReport.Api.Mappers;
using HeimReport.Api.Repositories.Employees;

namespace HeimReport.Api.Services.Employees;

public class EmployeeService(IEmployeeRepository employeeRepository) : IEmployeeService
{
    public async Task<(IEnumerable<EmployeeResponseDto> Items, int TotalCount)> GetAllAsync(
        EmployeeStatus? status,
        int? departmentId,
        int? positionId,
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        var (items, totalCount) = await employeeRepository.GetAllWithFiltersAsync(
            status, departmentId, positionId, pageNumber, pageSize, cancellationToken);

        var dtos = items.Select(e => e.ToResponseDto());
        return (dtos, totalCount);
    }

    public async Task<EmployeeResponseDto> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var employee = await employeeRepository.GetByIdWithDetailsAsync(id, cancellationToken)
            ?? throw NotFoundException.ForEntity<Employee>(id);

        return employee.ToResponseDto();
    }

    public async Task<EmployeeResponseDto> CreateAsync(EmployeeCreateDto dto, CancellationToken cancellationToken = default)
    {
        var normalizedEmail = dto.Email.Trim().ToUpperInvariant();
        if (await employeeRepository.GetByNormalizedEmailAsync(normalizedEmail, cancellationToken) != null)
            throw DomainException.AlreadyExists("Employee", "Email", dto.Email);

        if (await employeeRepository.ExistsByNationalIdAndCountryAsync(dto.NationalId, dto.CountryId, cancellationToken))
            throw DomainException.AlreadyExists("Employee", "NationalId", dto.NationalId);

        var employee = dto.ToEntity();

        await employeeRepository.AddAsync(employee, cancellationToken);
        await employeeRepository.SaveChangesAsync(cancellationToken);

        return employee.ToResponseDto();
    }

    public async Task UpdateAsync(int id, EmployeeUpdateDto dto, CancellationToken cancellationToken = default)
    {
        var employee = await employeeRepository.GetByIdAsync(id, cancellationToken)
            ?? throw NotFoundException.ForEntity<Employee>(id);

        var normalizedNewEmail = dto.Email.Trim().ToUpperInvariant();
        if (employee.NormalizedEmail != normalizedNewEmail &&
            await employeeRepository.GetByNormalizedEmailAsync(normalizedNewEmail, cancellationToken) != null)
        {
            throw DomainException.AlreadyExists("Employee", "Email", dto.Email);
        }

        if (employee.NationalId != dto.NationalId &&
            await employeeRepository.ExistsByNationalIdAndCountryAsync(dto.NationalId, dto.CountryId, cancellationToken))
        {
            throw DomainException.AlreadyExists("Employee", "NationalId", dto.NationalId);
        }

        employee.ApplyUpdate(dto);

        employeeRepository.Update(employee);
        await employeeRepository.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var employee = await employeeRepository.GetByIdAsync(id, cancellationToken)
            ?? throw NotFoundException.ForEntity<Employee>(id);

        employeeRepository.Remove(employee);
        await employeeRepository.SaveChangesAsync(cancellationToken);
    }
}