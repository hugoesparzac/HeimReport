using HeimReport.Api.Enums;

namespace HeimReport.Api.DTOs.Employees;

public record EmployeeSnapshot(
    int DepartmentId,
    int PositionId,
    int? ManagerId,
    decimal CurrentSalary,
    EmployeeStatus Status
);