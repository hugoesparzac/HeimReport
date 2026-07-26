using HeimReport.Api.DTOs.Common;
using HeimReport.Api.DTOs.Employees;
using HeimReport.Api.Entities;
using HeimReport.Api.Enums;
using HeimReport.Api.Exceptions;
using HeimReport.Api.Mappers;
using HeimReport.Api.Repositories.Employees;
using HeimReport.Api.Services.AuditLogs;
using HeimReport.Api.Storage;
using Microsoft.EntityFrameworkCore;

namespace HeimReport.Api.Services.Employees;

public class EmployeeService(
    IEmployeeRepository employeeRepository,
    IEmployeeJobHistoryRepository jobHistoryRepository,
    IAuditLogService auditLogService,
    IPhotoStorageService photoStorageService) : IEmployeeService
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

        await auditLogService.LogAsync(
            "CREATE_EMPLOYEE", nameof(Employee), employee.Id,
            oldValues: null,
            newValues: new
            {
                employee.FirstName,
                employee.LastName,
                employee.Email,
                employee.DepartmentId,
                employee.PositionId,
                employee.CountryId,
                employee.CurrentSalary,
                employee.ManagerId
            },
            cancellationToken: cancellationToken);

        var created = await employeeRepository.GetByIdWithDetailsAsync(employee.Id, cancellationToken)
            ?? throw NotFoundException.ForEntity<Employee>(employee.Id);

        return created.ToResponseDto();
    }

    public async Task UpdateAsync(int id, EmployeeUpdateDto dto, CancellationToken cancellationToken = default)
    {
        var employee = await employeeRepository.GetByIdAsync(id, cancellationToken)
            ?? throw NotFoundException.ForEntity<Employee>(id);

        var oldValues = new
        {
            employee.DepartmentId,
            employee.PositionId,
            employee.ManagerId,
            employee.CurrentSalary,
            employee.Status
        };

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

        var action = isNowOnLeave ? "TERMINATE_EMPLOYEE" : isReactivation ? "REACTIVATE_EMPLOYEE" : "UPDATE_EMPLOYEE";

        await auditLogService.LogAsync(
            action, nameof(Employee), id,
            oldValues,
            new
            {
                dto.DepartmentId,
                dto.PositionId,
                dto.ManagerId,
                dto.CurrentSalary,
                dto.Status,
                dto.TerminationDate
            },
            cancellationToken);
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

            var oldStatus = employee.Status;

            await CloseJobHistoryForTerminationAsync(id, dto.TerminationDate, cancellationToken);

            employee.Status = dto.Status;
            employee.TerminationDate = dto.TerminationDate ?? DateTime.UtcNow;
            employeeRepository.Update(employee);

            await auditLogService.LogAsync(
                "TERMINATE_EMPLOYEE", nameof(Employee), id,
                oldValues: new { Status = oldStatus },
                newValues: new { dto.Status, employee.TerminationDate },
                cancellationToken: cancellationToken);

            succeeded.Add(id);
        }

        await employeeRepository.SaveChangesAsync(cancellationToken);

        return new BulkOperationResultDto { SucceededIds = succeeded, Failed = failed };
    }

    public async Task<EmployeeResponseDto> UploadPhotoAsync(
        int id, IFormFile photo, CancellationToken cancellationToken = default)
    {
        var employee = await employeeRepository.GetByIdAsync(id, cancellationToken)
            ?? throw NotFoundException.ForEntity<Employee>(id);

        if (!string.IsNullOrEmpty(employee.PhotoPublicId))
        {
            await photoStorageService.DeleteAsync(employee.PhotoPublicId, cancellationToken);
        }

        await using var stream = photo.OpenReadStream();
        var result = await photoStorageService.UploadAsync(stream, photo.FileName, cancellationToken);

        var oldValues = new { employee.PhotoUrl };

        employee.PhotoUrl = result.Url;
        employee.PhotoPublicId = result.PublicId;

        employeeRepository.Update(employee);
        await employeeRepository.SaveChangesAsync(cancellationToken);

        await auditLogService.LogAsync(
            "UPDATE_EMPLOYEE_PHOTO", nameof(Employee), id,
            oldValues, new { employee.PhotoUrl },
            cancellationToken);

        var updated = await employeeRepository.GetByIdWithDetailsAsync(id, cancellationToken)
            ?? throw NotFoundException.ForEntity<Employee>(id);

        return updated.ToResponseDto();
    }

    public async Task RemovePhotoAsync(int id, CancellationToken cancellationToken = default)
    {
        var employee = await employeeRepository.GetByIdAsync(id, cancellationToken)
            ?? throw NotFoundException.ForEntity<Employee>(id);

        if (string.IsNullOrEmpty(employee.PhotoPublicId))
        {
            return;
        }

        await photoStorageService.DeleteAsync(employee.PhotoPublicId, cancellationToken);

        var oldValues = new { employee.PhotoUrl };

        employee.PhotoUrl = null;
        employee.PhotoPublicId = null;

        employeeRepository.Update(employee);
        await employeeRepository.SaveChangesAsync(cancellationToken);

        await auditLogService.LogAsync(
            "REMOVE_EMPLOYEE_PHOTO", nameof(Employee), id,
            oldValues, newValues: null,
            cancellationToken: cancellationToken);
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