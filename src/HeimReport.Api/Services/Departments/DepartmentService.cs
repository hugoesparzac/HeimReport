using HeimReport.Api.DTOs.Common;
using HeimReport.Api.DTOs.Departments;
using HeimReport.Api.Entities;
using HeimReport.Api.Exceptions;
using HeimReport.Api.Mappers;
using HeimReport.Api.Repositories.Departments;
using Microsoft.EntityFrameworkCore;

namespace HeimReport.Api.Services.Departments;

public class DepartmentService(IDepartmentRepository departmentRepository) : IDepartmentService
{
    public Task<PagedResultDto<DepartmentResponseDto>> GetActivePagedAsync(
        PaginationQueryDto query, CancellationToken cancellationToken = default)
    {
        return GetPagedAsync(isActive: true, query, cancellationToken);
    }

    public Task<PagedResultDto<DepartmentResponseDto>> GetInactivePagedAsync(
        PaginationQueryDto query, CancellationToken cancellationToken = default)
    {
        return GetPagedAsync(isActive: false, query, cancellationToken);
    }

    private async Task<PagedResultDto<DepartmentResponseDto>> GetPagedAsync(
        bool isActive, PaginationQueryDto query, CancellationToken cancellationToken)
    {
        var baseQuery = departmentRepository.Query()
            .Where(d => d.IsActive == isActive)
            .OrderBy(d => d.Name);

        var totalCount = await baseQuery.CountAsync(cancellationToken);

        var entities = await baseQuery
            .Skip((query.PageNumber - 1) * query.PageSize)
            .Take(query.PageSize)
            .ToListAsync(cancellationToken);

        return new PagedResultDto<DepartmentResponseDto>
        {
            Items = [.. entities.Select(d => d.ToResponseDto())],
            TotalCount = totalCount,
            PageNumber = query.PageNumber,
            PageSize = query.PageSize
        };
    }

    public async Task<DepartmentResponseDto> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var department = await departmentRepository.GetByIdAsync(id, cancellationToken)
            ?? throw NotFoundException.ForEntity<Department>(id);

        return department.ToResponseDto();
    }

    public async Task<DepartmentResponseDto> CreateAsync(DepartmentCreateDto dto, CancellationToken cancellationToken = default)
    {
        var department = dto.ToEntity();

        await departmentRepository.AddAsync(department, cancellationToken);
        await departmentRepository.SaveChangesAsync(cancellationToken);

        return department.ToResponseDto();
    }

    public async Task UpdateAsync(int id, DepartmentUpdateDto dto, CancellationToken cancellationToken = default)
    {
        var department = await departmentRepository.GetByIdAsync(id, cancellationToken)
            ?? throw NotFoundException.ForEntity<Department>(id);

        var isDeactivating = department.IsActive && !dto.IsActive;
        if (isDeactivating && await departmentRepository.IsReferencedByActiveEmployeeAsync(id, cancellationToken))
        {
            throw DomainException.EntityInUse("Department", "it is currently assigned to one or more active employees");
        }

        dto.UpdateEntity(department);

        departmentRepository.Update(department);
        await departmentRepository.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var department = await departmentRepository.GetByIdAsync(id, cancellationToken)
            ?? throw NotFoundException.ForEntity<Department>(id);

        if (!department.IsActive)
        {
            return;
        }

        if (await departmentRepository.IsReferencedByActiveEmployeeAsync(id, cancellationToken))
        {
            throw DomainException.EntityInUse("Department", "it is currently assigned to one or more active employees");
        }

        department.IsActive = false;

        departmentRepository.Update(department);
        await departmentRepository.SaveChangesAsync(cancellationToken);
    }

    public async Task ReactivateAsync(int id, CancellationToken cancellationToken = default)
    {
        var department = await departmentRepository.GetByIdAsync(id, cancellationToken)
            ?? throw NotFoundException.ForEntity<Department>(id);

        if (department.IsActive)
        {
            return;
        }

        department.IsActive = true;

        departmentRepository.Update(department);
        await departmentRepository.SaveChangesAsync(cancellationToken);
    }

    public async Task<BulkOperationResultDto> DeleteManyAsync(
        BulkIdsDto dto, CancellationToken cancellationToken = default)
    {
        var succeeded = new List<int>();
        var failed = new List<BulkOperationErrorDto>();

        foreach (var id in dto.Ids)
        {
            try
            {
                await DeleteAsync(id, cancellationToken);
                succeeded.Add(id);
            }
            catch (NotFoundException)
            {
                failed.Add(new BulkOperationErrorDto { Id = id, Reason = "Department not found" });
            }
            catch (DomainException ex)
            {
                failed.Add(new BulkOperationErrorDto { Id = id, Reason = ex.Message });
            }
        }

        return new BulkOperationResultDto { SucceededIds = succeeded, Failed = failed };
    }

    public async Task<BulkOperationResultDto> ReactivateManyAsync(
        BulkIdsDto dto, CancellationToken cancellationToken = default)
    {
        var succeeded = new List<int>();
        var failed = new List<BulkOperationErrorDto>();

        foreach (var id in dto.Ids)
        {
            try
            {
                await ReactivateAsync(id, cancellationToken);
                succeeded.Add(id);
            }
            catch (NotFoundException)
            {
                failed.Add(new BulkOperationErrorDto { Id = id, Reason = "Department not found" });
            }
        }

        return new BulkOperationResultDto { SucceededIds = succeeded, Failed = failed };
    }
}