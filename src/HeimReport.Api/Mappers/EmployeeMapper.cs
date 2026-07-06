using HeimReport.Api.DTOs.Employees;
using HeimReport.Api.Entities;

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
            Role = employee.Role,

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
    
    public static Employee ToEntity(this EmployeeCreateUpdateDto dto)
    {
        return new Employee
        {
            FirstName = dto.FirstName,
            LastName = dto.LastName,
            Email = dto.Email,
            NormalizedEmail = dto.Email.ToUpper(), 
            NationalId = dto.NationalId,
            HireDate = dto.HireDate,
            ContractType = dto.ContractType,
            ContractEndDate = dto.ContractEndDate,
            Status = dto.Status,
            Role = dto.Role,
            CountryId = dto.CountryId,
            DepartmentId = dto.DepartmentId,
            PositionId = dto.PositionId,
            ManagerId = dto.ManagerId
        };
    }
}