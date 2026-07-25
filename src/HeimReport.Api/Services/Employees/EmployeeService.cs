using HeimReport.Api.DTOs.Common;
using HeimReport.Api.DTOs.Employees;
using HeimReport.Api.Entities;
using HeimReport.Api.Enums;
using HeimReport.Api.Exceptions;
using HeimReport.Api.Mappers;
using HeimReport.Api.Repositories.Employees;
using Microsoft.EntityFrameworkCore;

namespace HeimReport.Api.Services.Employees;

public class EmployeeService(
    IEmployeeRepository employeeRepository,
    IEmployeeJobHistoryRepository jobHistoryRepository) : IEmployeeService
{
    public async Task<PagedResultDto<EmployeeResponseDto>> GetPagedAsync(
        EmployeeQueryDto query, CancellationToken cancellationToken = default)
    {
        var baseQuery = employeeRepository.QueryWithDetails();

        if (!string.IsNullOrWhiteSpace(query.SearchText))
        {
            var pattern = $"%{query.SearchText}%";
            baseQuery = baseQuery.Where(e =>
                EF.Functions.ILike(e.FirstName, pattern) ||
                EF.Functions.ILike(e.LastName, pattern) ||
                EF.Functions.ILike(e.Email, pattern));
        }

        if (query.CountryId.HasValue)
        {
            baseQuery = baseQuery.Where(e => e.CountryId == query.CountryId.Value);
        }

        if (query.DepartmentId.HasValue)
        {
            baseQuery = baseQuery.Where(e => e.DepartmentId == query.DepartmentId.Value);
        }

        if (query.PositionId.HasValue)
        {
            baseQuery = baseQuery.Where(e => e.PositionId == query.PositionId.Value);
        }

        if (query.Status.HasValue)
        {
            baseQuery = baseQuery.Where(e => e.Status == query.Status.Value);
        }

        baseQuery = baseQuery.OrderBy(e => e.LastName).ThenBy(e => e.FirstName);

        var totalCount = await baseQuery.CountAsync(cancellationToken);

        var entities = await baseQuery
            .Skip((query.PageNumber - 1) * query.PageSize)
            .Take(query.PageSize)
            .ToListAsync(cancellationToken);

        return new PagedResultDto<EmployeeResponseDto>
        {
            Items = [.. entities.Select(e => e.ToResponseDto())],
            TotalCount = totalCount,
            PageNumber = query.PageNumber,
            PageSize = query.PageSize
        };
    }

    public async Task<EmployeeResponseDto> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var employee = await employeeRepository.GetByIdWithDetailsAsync(id, cancellationToken)
            ?? throw NotFoundException.ForEntity<Employee>(id);

        return employee.ToResponseDto();
    }

    public async Task<EmployeeResponseDto> CreateAsync(
        EmployeeCreateDto dto, CancellationToken cancellationToken = default)
    {
        var employee = dto.ToEntity();

        await employeeRepository.AddAsync(employee, cancellationToken);
        await employeeRepository.SaveChangesAsync(cancellationToken);

        await jobHistoryRepository.AddAsync(new EmployeeJobHistory
        {
            EmployeeId = employee.Id,
            DepartmentId = employee.DepartmentId,
            PositionId = employee.PositionId,
            ManagerId = employee.ManagerId,
            Salary = employee.CurrentSalary,
            StartDate = employee.HireDate,
            ChangeReason = null,
            CreatedAt = DateTime.UtcNow
        }, cancellationToken);

        await jobHistoryRepository.SaveChangesAsync(cancellationToken);

        // TODO: AuditLogs (Action = "CREATE_EMPLOYEE") — pending, deferred cross-section.

        var created = await employeeRepository.GetByIdWithDetailsAsync(employee.Id, cancellationToken)
            ?? throw NotFoundException.ForEntity<Employee>(employee.Id);

        return created.ToResponseDto();
    }

    public async Task UpdateAsync(int id, EmployeeUpdateDto dto, CancellationToken cancellationToken = default)
    {
        var employee = await employeeRepository.GetByIdAsync(id, cancellationToken)
            ?? throw NotFoundException.ForEntity<Employee>(id);

        var wasOnLeave = IsOnLeave(employee.Status);
        var isNowOnLeave = employee.Status == EmployeeStatus.Active && IsOnLeave(dto.Status);
        var isReactivation = wasOnLeave && dto.Status == EmployeeStatus.Active;

        var hasJobFieldChange = dto.DepartmentId != employee.DepartmentId
            || dto.PositionId != employee.PositionId
            || dto.ManagerId != employee.ManagerId
            || dto.CurrentSalary != employee.CurrentSalary;

        if (isNowOnLeave)
        {
            await CloseJobHistoryForTerminationAsync(id, dto.TerminationDate, cancellationToken);
        }
        else if (isReactivation)
        {
            await OpenJobHistoryAsync(id, dto.DepartmentId, dto.PositionId, dto.ManagerId, dto.CurrentSalary,
                JobChangeReason.Reactivation, otherReasonDetail: null, cancellationToken);
        }
        else if (hasJobFieldChange)
        {
            await CloseOpenJobHistoryAsync(id, cancellationToken);
            await OpenJobHistoryAsync(id, dto.DepartmentId, dto.PositionId, dto.ManagerId, dto.CurrentSalary,
                dto.ChangeReason, dto.ChangeReason == JobChangeReason.Other ? dto.OtherReasonDetail : null, cancellationToken);
        }

        dto.UpdateEntity(employee);
        employeeRepository.Update(employee);

        await employeeRepository.SaveChangesAsync(cancellationToken);

        // TODO: AuditLogs (Action = "UPDATE_EMPLOYEE", OldValues/NewValues) — pending.
    }

    public async Task<BulkOperationResultDto> TerminateManyAsync(
        EmployeeBulkTerminationDto dto, CancellationToken cancellationToken = default)
    {
        var succeeded = new List<int>();
        var failed = new List<BulkOperationErrorDto>();

        foreach (var id in dto.EmployeeIds)
        {
            var employee = await employeeRepository.GetByIdAsync(id, cancellationToken);

            if (employee is null)
            {
                failed.Add(new BulkOperationErrorDto { Id = id, Reason = "Employee not found" });
                continue;
            }

            if (employee.Status != EmployeeStatus.Active)
            {
                failed.Add(new BulkOperationErrorDto { Id = id, Reason = "Employee is not currently active" });
                continue;
            }

            await CloseJobHistoryForTerminationAsync(id, dto.TerminationDate, cancellationToken);

            employee.Status = dto.Status;
            employee.TerminationDate = dto.TerminationDate ?? DateTime.UtcNow;
            employeeRepository.Update(employee);

            succeeded.Add(id);
        }

        await employeeRepository.SaveChangesAsync(cancellationToken);

        // TODO: Audit logs for each employee who has been terminated — pending.

        return new BulkOperationResultDto { SucceededIds = succeeded, Failed = failed };
    }

    private static bool IsOnLeave(EmployeeStatus status) =>
        status is EmployeeStatus.VoluntaryResignation or EmployeeStatus.InvoluntaryTermination or EmployeeStatus.ContractExpired;

    private async Task CloseJobHistoryForTerminationAsync(
        int employeeId, DateTime? terminationDate, CancellationToken cancellationToken)
    {
        var currentHistory = await jobHistoryRepository.GetOpenRecordAsync(employeeId, cancellationToken);
        if (currentHistory is null)
        {
            return;
        }

        currentHistory.EndDate = terminationDate ?? DateTime.UtcNow;
        currentHistory.ChangeReason = JobChangeReason.Termination;
        jobHistoryRepository.Update(currentHistory);
    }

    private async Task CloseOpenJobHistoryAsync(int employeeId, CancellationToken cancellationToken)
    {
        var currentHistory = await jobHistoryRepository.GetOpenRecordAsync(employeeId, cancellationToken);
        if (currentHistory is null)
        {
            return;
        }

        currentHistory.EndDate = DateTime.UtcNow;
        jobHistoryRepository.Update(currentHistory);
    }

    private Task OpenJobHistoryAsync(
        int employeeId, int departmentId, int positionId, int? managerId, decimal salary,
        JobChangeReason? changeReason, string? otherReasonDetail, CancellationToken cancellationToken)
    {
        return jobHistoryRepository.AddAsync(new EmployeeJobHistory
        {
            EmployeeId = employeeId,
            DepartmentId = departmentId,
            PositionId = positionId,
            ManagerId = managerId,
            Salary = salary,
            StartDate = DateTime.UtcNow,
            ChangeReason = changeReason,
            OtherReasonDetail = otherReasonDetail,
            CreatedAt = DateTime.UtcNow
        }, cancellationToken);
    }
}