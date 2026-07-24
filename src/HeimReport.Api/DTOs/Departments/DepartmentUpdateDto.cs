namespace HeimReport.Api.DTOs.Departments;

public record DepartmentUpdateDto
{
    public required int Id { get; init; }
    public required string Name { get; init; }
    public bool IsActive { get; init; }
}
