using HeimReport.Api.Enums;

namespace HeimReport.Api.DTOs.Employees;

public record EmployeeResponseDto
{
    public int Id { get; init; }
    public required string FirstName { get; init; }
    public required string LastName { get; init; }
    public required string Email { get; init; }
    public required string NationalId { get; init; }
    public DateTime HireDate { get; init; }
    public ContractType ContractType { get; init; }
    public DateTime? ContractEndDate { get; init; }
    public DateTime? TerminationDate { get; init; }
    public EmployeeStatus Status { get; init; }
    public SystemRole Role { get; init; }
    
    public int CountryId { get; init; }
    public string CountryName { get; init; } = string.Empty;
    
    public int DepartmentId { get; init; }
    public string DepartmentName { get; init; } = string.Empty;
    
    public int PositionId { get; init; }
    public string PositionTitle { get; init; } = string.Empty;
    
    public int? ManagerId { get; init; }
    public string? ManagerFullName { get; init; }
}