namespace HeimReport.Api.DTOs.Departments;

public record DepartmentCreateUpdateDto
{
    public required string Name { get; init; }
    public bool IsActive { get; init; } = true;
}