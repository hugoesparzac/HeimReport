using HeimReport.Api.Enums;

namespace HeimReport.Api.DTOs.Employees;

public record EmployeeBulkTerminationDto
{
    public required IReadOnlyList<int> EmployeeIds { get; init; }
    public required EmployeeStatus Status { get; init; }
    public DateTime? TerminationDate { get; init; }
}