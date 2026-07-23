using HeimReport.Api.Enums;
namespace HeimReport.Api.DTOs.Employees;

public record EmployeeCreateDto
{
    public required string FirstName { get; init; }
    public required string LastName { get; init; }
    public required string Email { get; init; }
    public required string NationalId { get; init; }
    public required DateTime BirthDate { get; init; }
    public required DateTime HireDate { get; init; }
    public required ContractType ContractType { get; init; }
    public DateTime? ContractEndDate { get; init; }
    public required decimal CurrentSalary { get; init; }
    public required int CountryId { get; init; }
    public required int DepartmentId { get; init; }
    public required int PositionId { get; init; }
    public int? ManagerId { get; init; }
}