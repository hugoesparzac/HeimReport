using HeimReport.Api.DTOs.Common;
using HeimReport.Api.DTOs.Countries;
using HeimReport.Api.Entities;
using HeimReport.Api.Exceptions;
using HeimReport.Api.Mappers;
using HeimReport.Api.Repositories.Countries;
using Microsoft.EntityFrameworkCore;

namespace HeimReport.Api.Services.Countries;

public class CountryService(ICountryRepository countryRepository) : ICountryService
{
    public Task<PagedResultDto<CountryResponseDto>> GetActivePagedAsync(
        PaginationQueryDto query, CancellationToken cancellationToken = default)
    {
        return GetPagedAsync(isActive: true, query, cancellationToken);
    }

    public Task<PagedResultDto<CountryResponseDto>> GetInactivePagedAsync(
        PaginationQueryDto query, CancellationToken cancellationToken = default)
    {
        return GetPagedAsync(isActive: false, query, cancellationToken);
    }

    private async Task<PagedResultDto<CountryResponseDto>> GetPagedAsync(
        bool isActive, PaginationQueryDto query, CancellationToken cancellationToken)
    {
        var baseQuery = countryRepository.Query()
            .Where(c => c.IsActive == isActive)
            .OrderBy(c => c.Name);

        var totalCount = await baseQuery.CountAsync(cancellationToken);

        var entities = await baseQuery
            .Skip((query.PageNumber - 1) * query.PageSize)
            .Take(query.PageSize)
            .ToListAsync(cancellationToken);

        return new PagedResultDto<CountryResponseDto>
        {
            Items = [.. entities.Select(c => c.ToResponseDto())],
            TotalCount = totalCount,
            PageNumber = query.PageNumber,
            PageSize = query.PageSize
        };
    }

    public async Task<CountryResponseDto> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var country = await countryRepository.GetByIdAsync(id, cancellationToken)
            ?? throw NotFoundException.ForEntity<Country>(id);

        return country.ToResponseDto();
    }

    public async Task<CountryResponseDto> CreateAsync(CountryCreateDto dto, CancellationToken cancellationToken = default)
    {
        var country = dto.ToEntity();

        await countryRepository.AddAsync(country, cancellationToken);
        await countryRepository.SaveChangesAsync(cancellationToken);

        return country.ToResponseDto();
    }

    public async Task UpdateAsync(int id, CountryUpdateDto dto, CancellationToken cancellationToken = default)
    {
        var country = await countryRepository.GetByIdAsync(id, cancellationToken)
            ?? throw NotFoundException.ForEntity<Country>(id);

        var isDeactivating = country.IsActive && !dto.IsActive;
        if (isDeactivating && await countryRepository.IsReferencedByActiveEmployeeAsync(id, cancellationToken))
        {
            throw DomainException.EntityInUse("Country", "it is currently assigned to one or more active employees");
        }

        dto.UpdateEntity(country);

        countryRepository.Update(country);
        await countryRepository.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var country = await countryRepository.GetByIdAsync(id, cancellationToken)
            ?? throw NotFoundException.ForEntity<Country>(id);

        if (!country.IsActive)
        {
            return;
        }

        if (await countryRepository.IsReferencedByActiveEmployeeAsync(id, cancellationToken))
        {
            throw DomainException.EntityInUse("Country", "it is currently assigned to one or more active employees");
        }

        country.IsActive = false;

        countryRepository.Update(country);
        await countryRepository.SaveChangesAsync(cancellationToken);
    }

    public async Task ReactivateAsync(int id, CancellationToken cancellationToken = default)
    {
        var country = await countryRepository.GetByIdAsync(id, cancellationToken)
            ?? throw NotFoundException.ForEntity<Country>(id);

        if (country.IsActive)
        {
            return;
        }

        country.IsActive = true;

        countryRepository.Update(country);
        await countryRepository.SaveChangesAsync(cancellationToken);
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
                failed.Add(new BulkOperationErrorDto { Id = id, Reason = "Country not found" });
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
                failed.Add(new BulkOperationErrorDto { Id = id, Reason = "Country not found" });
            }
        }

        return new BulkOperationResultDto { SucceededIds = succeeded, Failed = failed };
    }
}