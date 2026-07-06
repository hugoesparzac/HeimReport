using System.ComponentModel.DataAnnotations;
using HeimReport.Api.Enums;

namespace HeimReport.Api.DTOs.Employees;

public record EmployeeCreateUpdateDto
{
    [Required(ErrorMessage = "First Name is required")]
    [MaxLength(100)]
    public required string FirstName { get; init; }
    
    [Required(ErrorMessage = "Last Name is required")]
    [MaxLength(100)]
    public required string LastName { get; init; }
    
    [Required(ErrorMessage = "Email is required")]
    [EmailAddress(ErrorMessage = "Invalid email format")]
    [MaxLength(100)]
    public required string Email { get; init; }
    
    [Required(ErrorMessage = "National ID is required")]
    [MaxLength(50)]
    public required string NationalId { get; init; }
    
    [Required(ErrorMessage = "Hire Date is required")]
    public required DateTime HireDate { get; init; }
    
    [Required(ErrorMessage = "Contract Type is required")]
    public required ContractType ContractType { get; init; }
    
    public DateTime? ContractEndDate { get; init; }
    public EmployeeStatus Status { get; init; }
    
    [Required]
    public required int CountryId { get; init; }
    
    [Required]
    public required int DepartmentId { get; init; }
    
    [Required]
    public required int PositionId { get; init; }
    
    public int? ManagerId { get; init; }
}