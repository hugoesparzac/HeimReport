using HeimReport.Api.DTOs.Employees;
using HeimReport.Api.Entities;
using HeimReport.Api.Enums;

namespace HeimReport.Api.Mappers;
public static class EmployeeMapper
{
    public static EmployeeResponseDto ToResponseDto(this Employee employee)
    {
        return new EmployeeResponseDto
        {
            Id = employee.Id,
            FirstName = employee.FirstName,
            LastName = employee.LastName,
            Email = employee.Email,
            NationalId = employee.NationalId,
            HireDate = employee.HireDate,
            ContractType = employee.ContractType,
            ContractEndDate = employee.ContractEndDate,
            TerminationDate = employee.TerminationDate,
            Status = employee.Status,
            CountryId = employee.CountryId,
            CountryName = employee.Country?.Name ?? string.Empty,
            DepartmentId = employee.DepartmentId,
            DepartmentName = employee.Department?.Name ?? string.Empty,
            PositionId = employee.PositionId,
            PositionTitle = employee.Position?.Title ?? string.Empty,
            ManagerId = employee.ManagerId,
            ManagerFullName = employee.Manager != null
                ? $"{employee.Manager.FirstName} {employee.Manager.LastName}"
                : null
        };
    }

    public static Employee ToEntity(this EmployeeCreateDto dto)
    {
        return new Employee
        {
            FirstName = dto.FirstName,
            LastName = dto.LastName,
            Email = dto.Email,
            NormalizedEmail = dto.Email.Trim().ToUpperInvariant(),
            NationalId = dto.NationalId,
            HireDate = dto.HireDate,
            ContractType = dto.ContractType,
            ContractEndDate = dto.ContractEndDate,
            Status = EmployeeStatus.Active,
            CountryId = dto.CountryId,
            DepartmentId = dto.DepartmentId,
            PositionId = dto.PositionId,
            ManagerId = dto.ManagerId,
            CreatedAt = DateTime.UtcNow
        };
    }

    public static void ApplyUpdate(this Employee employee, EmployeeUpdateDto dto)
    {
        employee.FirstName = dto.FirstName;
        employee.LastName = dto.LastName;
        employee.Email = dto.Email;
        employee.NormalizedEmail = dto.Email.Trim().ToUpperInvariant();
        employee.NationalId = dto.NationalId;
        employee.ContractType = dto.ContractType;
        employee.ContractEndDate = dto.ContractEndDate;
        employee.Status = dto.Status;
        employee.TerminationDate = dto.TerminationDate;
        employee.CountryId = dto.CountryId;
        employee.DepartmentId = dto.DepartmentId;
        employee.PositionId = dto.PositionId;
        employee.ManagerId = dto.ManagerId;
    }
}