using HeimReport.Api.DTOs.Employees;
using HeimReport.Api.Entities;

namespace HeimReport.Api.Mappers;

public static class EmployeeMapper
{
    public static EmployeeResponseDto ToResponseDto(this Employee employee)
    {
        if (employee.Country is null || employee.Department is null || employee.Position is null)
        {
            throw new InvalidOperationException(
                $"Employee with Id {employee.Id} was loaded without its related Country/Department/Position. " +
                "Ensure the query includes .Include(e => e.Country).Include(e => e.Department).Include(e => e.Position).");
        }

        if (employee.ManagerId.HasValue && employee.Manager is null)
        {
            throw new InvalidOperationException(
                $"Employee with Id {employee.Id} has ManagerId {employee.ManagerId} but the Manager navigation was not loaded. " +
                "Ensure the query includes .Include(e => e.Manager).");
        }

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
            CountryName = employee.Country.Name,
            DepartmentId = employee.DepartmentId,
            DepartmentName = employee.Department.Name,
            PositionId = employee.PositionId,
            PositionTitle = employee.Position.Title,
            ManagerId = employee.ManagerId,
            ManagerFullName = employee.Manager is not null
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
            NormalizedEmail = dto.Email.ToUpperInvariant(),
            NationalId = dto.NationalId,
            BirthDate = dto.BirthDate,
            HireDate = dto.HireDate,
            ContractType = dto.ContractType,
            ContractEndDate = dto.ContractEndDate,
            Status = HeimReport.Api.Enums.EmployeeStatus.Active,
            CurrentSalary = dto.CurrentSalary,
            CountryId = dto.CountryId,
            DepartmentId = dto.DepartmentId,
            PositionId = dto.PositionId,
            ManagerId = dto.ManagerId
        };
    }

    public static void UpdateEntity(this EmployeeUpdateDto dto, Employee employee)
    {
        employee.FirstName = dto.FirstName;
        employee.LastName = dto.LastName;
        employee.Email = dto.Email;
        employee.NormalizedEmail = dto.Email.ToUpperInvariant();
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