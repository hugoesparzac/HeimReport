using HeimReport.Api.Enums;

namespace HeimReport.Api.Entities;

public class Employee
{
    public int Id { get; set; }
    public required string FirstName { get; set; }
    public required string LastName { get; set; }
    public required string Email { get; set; }
    public required string NormalizedEmail { get; set; }
    public required string NationalId { get; set; }
    public required DateTime HireDate { get; set; }
    public required ContractType ContractType { get; set; }
    public DateTime? ContractEndDate { get; set; }
    public DateTime? TerminationDate { get; set; }
    public EmployeeStatus Status { get; set; }
    public required int CountryId { get; set; }
    public required Country Country { get; set; }
    public required int DepartmentId { get; set; }
    public required Department Department { get; set; }
    public required int PositionId { get; set; }
    public required Position Position { get; set; }
    public int? ManagerId { get; set; }
    public Employee? Manager { get; set; }
    public ICollection<Employee>? DirectReports { get; set; }
    public required DateTime CreatedAt { get; set; }
}