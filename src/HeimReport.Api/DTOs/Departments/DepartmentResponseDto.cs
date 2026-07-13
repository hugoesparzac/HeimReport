namespace HeimReport.Api.DTOs.Departments;

public record DepartmentResponseDto
{
    public int Id { get; init; }
    public required string Name { get; init; }
}