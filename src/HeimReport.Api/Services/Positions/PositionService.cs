using HeimReport.Api.DTOs.Common;
using HeimReport.Api.DTOs.Positions;
using HeimReport.Api.Entities;
using HeimReport.Api.Exceptions;
using HeimReport.Api.Mappers;
using HeimReport.Api.Repositories.Positions;
using Microsoft.EntityFrameworkCore;

namespace HeimReport.Api.Services.Positions;

public class PositionService(IPositionRepository positionRepository) : IPositionService
{
    public Task<PagedResultDto<PositionResponseDto>> GetActivePagedAsync(
        PaginationQueryDto query, CancellationToken cancellationToken = default)
    {
        return GetPagedAsync(isActive: true, query, cancellationToken);
    }

    public Task<PagedResultDto<PositionResponseDto>> GetInactivePagedAsync(
        PaginationQueryDto query, CancellationToken cancellationToken = default)
    {
        return GetPagedAsync(isActive: false, query, cancellationToken);
    }

    private async Task<PagedResultDto<PositionResponseDto>> GetPagedAsync(
        bool isActive, PaginationQueryDto query, CancellationToken cancellationToken)
    {
        var baseQuery = positionRepository.Query()
            .Where(p => p.IsActive == isActive)
            .OrderBy(p => p.Title);

        var totalCount = await baseQuery.CountAsync(cancellationToken);

        var entities = await baseQuery
            .Skip((query.PageNumber - 1) * query.PageSize)
            .Take(query.PageSize)
            .ToListAsync(cancellationToken);

        return new PagedResultDto<PositionResponseDto>
        {
            Items = [.. entities.Select(p => p.ToResponseDto())],
            TotalCount = totalCount,
            PageNumber = query.PageNumber,
            PageSize = query.PageSize
        };
    }

    public async Task<PositionResponseDto> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var position = await positionRepository.GetByIdAsync(id, cancellationToken)
            ?? throw NotFoundException.ForEntity<Position>(id);

        return position.ToResponseDto();
    }

    public async Task<PositionResponseDto> CreateAsync(PositionCreateDto dto, CancellationToken cancellationToken = default)
    {
        var position = dto.ToEntity();

        await positionRepository.AddAsync(position, cancellationToken);
        await positionRepository.SaveChangesAsync(cancellationToken);

        return position.ToResponseDto();
    }

    public async Task UpdateAsync(int id, PositionUpdateDto dto, CancellationToken cancellationToken = default)
    {
        var position = await positionRepository.GetByIdAsync(id, cancellationToken)
            ?? throw NotFoundException.ForEntity<Position>(id);

        var isDeactivating = position.IsActive && !dto.IsActive;
        if (isDeactivating && await positionRepository.IsReferencedByActiveEmployeeAsync(id, cancellationToken))
        {
            throw DomainException.EntityInUse("Position", "it is currently assigned to one or more active employees");
        }

        dto.UpdateEntity(position);

        positionRepository.Update(position);
        await positionRepository.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var position = await positionRepository.GetByIdAsync(id, cancellationToken)
            ?? throw NotFoundException.ForEntity<Position>(id);

        if (!position.IsActive)
        {
            return;
        }

        if (await positionRepository.IsReferencedByActiveEmployeeAsync(id, cancellationToken))
        {
            throw DomainException.EntityInUse("Position", "it is currently assigned to one or more active employees");
        }

        position.IsActive = false;

        positionRepository.Update(position);
        await positionRepository.SaveChangesAsync(cancellationToken);
    }

    public async Task ReactivateAsync(int id, CancellationToken cancellationToken = default)
    {
        var position = await positionRepository.GetByIdAsync(id, cancellationToken)
            ?? throw NotFoundException.ForEntity<Position>(id);

        if (position.IsActive)
        {
            return;
        }

        position.IsActive = true;

        positionRepository.Update(position);
        await positionRepository.SaveChangesAsync(cancellationToken);
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
                failed.Add(new BulkOperationErrorDto { Id = id, Reason = "Position not found" });
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
                failed.Add(new BulkOperationErrorDto { Id = id, Reason = "Position not found" });
            }
        }

        return new BulkOperationResultDto { SucceededIds = succeeded, Failed = failed };
    }
}