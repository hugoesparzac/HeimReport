using HeimReport.Api.DTOs.Positions;
using HeimReport.Api.Exceptions;
using HeimReport.Api.Mappers;
using HeimReport.Api.Repositories.Positions;
using Microsoft.EntityFrameworkCore;

namespace HeimReport.Api.Services.Positions;

public class PositionService(IPositionRepository positionRepository) : IPositionService
{
    public async Task<IEnumerable<PositionResponseDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var positions = await positionRepository.Query().ToListAsync(cancellationToken);
        return positions.Select(p => p.ToResponseDto());
    }

    public async Task<PositionResponseDto> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var position = await positionRepository.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException($"Position with ID {id} was not found.");

        return position.ToResponseDto();
    }

    public async Task<PositionResponseDto> CreateAsync(PositionCreateUpdateDto dto, CancellationToken cancellationToken = default)
    {
        if (await positionRepository.ExistsByNameAsync(dto.Title, cancellationToken))
            throw DomainException.AlreadyExists("Position", "Title", dto.Title);

        var position = dto.ToEntity();

        await positionRepository.AddAsync(position, cancellationToken);
        await positionRepository.SaveChangesAsync(cancellationToken);

        return position.ToResponseDto();
    }

    public async Task UpdateAsync(int id, PositionCreateUpdateDto dto, CancellationToken cancellationToken = default)
    {
        var position = await positionRepository.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException($"Position with ID {id} was not found.");

        if (!string.Equals(position.Title, dto.Title, StringComparison.OrdinalIgnoreCase) &&
            await positionRepository.ExistsByNameAsync(dto.Title, cancellationToken))
        {
            throw DomainException.AlreadyExists("Position", "Title", dto.Title);
        }

        position.Title = dto.Title;

        positionRepository.Update(position);
        await positionRepository.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var position = await positionRepository.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException($"Position with ID {id} was not found.");

        if (await positionRepository.IsReferencedByEmployeeAsync(id, cancellationToken))
        {
            throw DomainException.EntityInUse("Position", "it is currently assigned to one or more employees");
        }

        positionRepository.Remove(position);
        await positionRepository.SaveChangesAsync(cancellationToken);
    }
}