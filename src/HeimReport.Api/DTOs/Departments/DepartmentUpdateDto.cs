namespace HeimReport.Api.DTOs.Departments;

public record DepartmentUpdateDto
{
    public required string Name { get; init; }
    public bool IsActive { get; init; }
}