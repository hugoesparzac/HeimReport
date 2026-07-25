using HeimReport.Api.DTOs.Common;
using HeimReport.Api.Enums;

namespace HeimReport.Api.DTOs.Employees;

public record EmployeeQueryDto : PaginationQueryDto
{
    public string? SearchText { get; init; }
    public int? CountryId { get; init; }
    public int? DepartmentId { get; init; }
    public int? PositionId { get; init; }
    public EmployeeStatus? Status { get; init; }
}