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
            BirthDate = employee.BirthDate,
            HireDate = employee.HireDate,
            ContractType = employee.ContractType,
            ContractEndDate = employee.ContractEndDate,
            TerminationDate = employee.TerminationDate,
            Status = employee.Status,
            CurrentSalary = employee.CurrentSalary,
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
            BirthDate = dto.BirthDate,
            HireDate = dto.HireDate,
            ContractType = dto.ContractType,
            ContractEndDate = dto.ContractEndDate,
            Status = EmployeeStatus.Active,
            CurrentSalary = dto.CurrentSalary,
            CountryId = dto.CountryId,
            DepartmentId = dto.DepartmentId,
            PositionId = dto.PositionId,
            ManagerId = dto.ManagerId
        };
    }

    public static void ApplyUpdate(this Employee employee, EmployeeUpdateDto dto)
    {
        employee.FirstName = dto.FirstName;
        employee.LastName = dto.LastName;
        employee.Email = dto.Email;
        employee.NormalizedEmail = dto.Email.Trim().ToUpperInvariant();
        employee.NationalId = dto.NationalId;
        employee.BirthDate = dto.BirthDate;
        employee.ContractType = dto.ContractType;
        employee.ContractEndDate = dto.ContractEndDate;
        employee.Status = dto.Status;
        employee.TerminationDate = dto.TerminationDate;
        employee.CurrentSalary = dto.CurrentSalary;
        employee.CountryId = dto.CountryId;
        employee.DepartmentId = dto.DepartmentId;
        employee.PositionId = dto.PositionId;
        employee.ManagerId = dto.ManagerId;
    }
}