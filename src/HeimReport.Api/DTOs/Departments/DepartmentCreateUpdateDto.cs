using System.ComponentModel.DataAnnotations;

namespace HeimReport.Api.DTOs.Departments;

public record DepartmentCreateUpdateDto(
    [Required(ErrorMessage =  "Name is required")]
    [MaxLength(100, ErrorMessage = "Name cannot exceed 100 characters")]
    string Name
);