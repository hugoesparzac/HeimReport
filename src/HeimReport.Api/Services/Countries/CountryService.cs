using HeimReport.Api.DTOs.Countries;
using HeimReport.Api.Exceptions;
using HeimReport.Api.Mappers;
using HeimReport.Api.Repositories.Countries;
using Microsoft.EntityFrameworkCore;

namespace HeimReport.Api.Services.Countries;

public class CountryService(ICountryRepository countryRepository) : ICountryService
{
    public async Task<IEnumerable<CountryResponseDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var countries = await countryRepository.Query().ToListAsync(cancellationToken);
        return countries.Select(c => c.ToResponseDto());
    }

    public async Task<CountryResponseDto> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var country = await countryRepository.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException($"Country with ID {id} was not found.");

        return country.ToResponseDto();
    }

    public async Task<CountryResponseDto> CreateAsync(CountryCreateUpdateDto dto, CancellationToken cancellationToken = default)
    {
        if (await countryRepository.ExistsByNameAsync(dto.Name, cancellationToken))
            throw DomainException.AlreadyExists("Country", "Name", dto.Name);

        var country = dto.ToEntity();

        await countryRepository.AddAsync(country, cancellationToken);
        await countryRepository.SaveChangesAsync(cancellationToken);

        return country.ToResponseDto();
    }

    public async Task UpdateAsync(int id, CountryCreateUpdateDto dto, CancellationToken cancellationToken = default)
    {
        var country = await countryRepository.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException($"Country with ID {id} was not found.");

        if (!string.Equals(country.Name, dto.Name, StringComparison.OrdinalIgnoreCase) &&
            await countryRepository.ExistsByNameAsync(dto.Name, cancellationToken))
        {
            throw DomainException.AlreadyExists("Country", "Name", dto.Name);
        }

        country.Name = dto.Name;

        countryRepository.Update(country);
        await countryRepository.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var country = await countryRepository.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException($"Country with ID {id} was not found.");

        if (await countryRepository.IsReferencedByEmployeeAsync(id, cancellationToken))
        {
            throw DomainException.EntityInUse("Country", "it is currently assigned to one or more employees");
        }

        countryRepository.Remove(country);
        await countryRepository.SaveChangesAsync(cancellationToken);
    }
}