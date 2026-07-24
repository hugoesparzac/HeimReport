namespace HeimReport.Api.DTOs.Departments;

public record DepartmentCreateDto
{
    public required string Name { get; init; }
    public bool IsActive { get; init; } = true;
}