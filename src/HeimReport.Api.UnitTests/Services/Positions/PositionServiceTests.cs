using Bogus;
using HeimReport.Api.DTOs.Common;
using HeimReport.Api.DTOs.Positions;
using HeimReport.Api.Entities;
using HeimReport.Api.Enums;
using HeimReport.Api.Exceptions;
using HeimReport.Api.Repositories.Positions;
using HeimReport.Api.Services.AuditLogs;
using HeimReport.Api.Services.Positions;
using MockQueryable;
using Moq;

namespace HeimReport.Api.UnitTests.Services.Positions;

public class PositionServiceTests
{
    private readonly Mock<IPositionRepository> _positionRepository = new();
    private readonly Mock<IAuditLogService> _auditLogService = new();

    private readonly PositionService _sut;

    public PositionServiceTests()
    {
        _auditLogService
            .Setup(a => a.LogAsync(
                It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int?>(),
                It.IsAny<object>(), It.IsAny<object>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _sut = new PositionService(_positionRepository.Object, _auditLogService.Object);
    }

    // ===================== GET ACTIVE / INACTIVE PAGED =====================

    [Fact]
    public async Task GetActivePagedAsync_ShouldReturnOnlyActivePositions()
    {
        // Arrange
        var positions = new List<Position>
        {
            GetPositionFaker(isActive: true).Generate(),
            GetPositionFaker(isActive: true).Generate(),
            GetPositionFaker(isActive: false).Generate()
        };

        _positionRepository.Setup(r => r.Query()).Returns(positions.BuildMock());

        var query = new PaginationQueryDto { PageNumber = 1, PageSize = 10 };

        // Act
        var result = await _sut.GetActivePagedAsync(query);

        // Assert
        Assert.Equal(2, result.TotalCount);
        Assert.All(result.Items, p => Assert.True(p.IsActive));
    }

    [Fact]
    public async Task GetInactivePagedAsync_ShouldReturnOnlyInactivePositions()
    {
        // Arrange
        var positions = new List<Position>
        {
            GetPositionFaker(isActive: true).Generate(),
            GetPositionFaker(isActive: false).Generate(),
            GetPositionFaker(isActive: false).Generate()
        };

        _positionRepository.Setup(r => r.Query()).Returns(positions.BuildMock());

        var query = new PaginationQueryDto { PageNumber = 1, PageSize = 10 };

        // Act
        var result = await _sut.GetInactivePagedAsync(query);

        // Assert
        Assert.Equal(2, result.TotalCount);
        Assert.All(result.Items, p => Assert.False(p.IsActive));
    }

    // ===================== GET BY ID =====================

    [Fact]
    public async Task GetByIdAsync_ShouldReturnPosition_WhenFound()
    {
        // Arrange
        var position = GetPositionFaker().Generate();

        _positionRepository
            .Setup(r => r.GetByIdAsync(position.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(position);

        // Act
        var result = await _sut.GetByIdAsync(position.Id);

        // Assert
        Assert.Equal(position.Id, result.Id);
        Assert.Equal(position.Title, result.Title);
    }

    [Fact]
    public async Task GetByIdAsync_ShouldThrow_WhenNotFound()
    {
        // Arrange
        _positionRepository
            .Setup(r => r.GetByIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Position?)null);

        // Act
        Task Act() => _sut.GetByIdAsync(1);

        // Assert
        await Assert.ThrowsAsync<NotFoundException>(Act);
    }

    // ===================== CREATE =====================

    [Fact]
    public async Task CreateAsync_ShouldAddPosition_AndLogAudit()
    {
        // Arrange
        var dto = new PositionCreateDto
        {
            Title = "QA Engineer",
            CareerLevel = CareerLevel.Professional,
            IsCritical = false,
            IsActive = true
        };

        // Act
        var result = await _sut.CreateAsync(dto);

        // Assert
        Assert.Equal(dto.Title, result.Title);
        Assert.Equal(dto.CareerLevel, result.CareerLevel);
        Assert.True(result.IsActive);

        _positionRepository.Verify(r => r.AddAsync(
            It.Is<Position>(p => p.Title == dto.Title && p.CareerLevel == dto.CareerLevel),
            It.IsAny<CancellationToken>()),
            Times.Once);

        _positionRepository.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);

        _auditLogService.Verify(a => a.LogAsync(
            "CREATE_POSITION", nameof(Position), It.IsAny<int?>(),
            null, It.IsAny<object>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    // ===================== UPDATE =====================

    [Fact]
    public async Task UpdateAsync_ShouldUpdateFields_WhenNotDeactivating()
    {
        // Arrange
        var position = GetPositionFaker(isActive: true).Generate();
        var dto = new PositionUpdateDto
        {
            Title = "Senior QA Engineer",
            CareerLevel = CareerLevel.SeniorProfessional,
            IsCritical = true,
            IsActive = true
        };

        _positionRepository
            .Setup(r => r.GetByIdAsync(position.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(position);

        // Act
        await _sut.UpdateAsync(position.Id, dto);

        // Assert
        Assert.Equal(dto.Title, position.Title);
        Assert.Equal(dto.CareerLevel, position.CareerLevel);
        Assert.True(position.IsCritical);

        _positionRepository.Verify(r => r.Update(position), Times.Once);
        _positionRepository.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);

        _auditLogService.Verify(a => a.LogAsync(
            "UPDATE_POSITION", nameof(Position), position.Id,
            It.IsAny<object>(), It.IsAny<object>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_ShouldThrow_WhenDeactivatingWithActiveEmployeesAssigned()
    {
        // Arrange
        var position = GetPositionFaker(isActive: true).Generate();
        var dto = new PositionUpdateDto
        {
            Title = position.Title,
            CareerLevel = position.CareerLevel,
            IsCritical = position.IsCritical,
            IsActive = false
        };

        _positionRepository
            .Setup(r => r.GetByIdAsync(position.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(position);

        _positionRepository
            .Setup(r => r.IsReferencedByActiveEmployeeAsync(position.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        Task Act() => _sut.UpdateAsync(position.Id, dto);

        // Assert
        await Assert.ThrowsAsync<DomainException>(Act);
        _positionRepository.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task UpdateAsync_ShouldThrow_WhenPositionNotFound()
    {
        // Arrange
        _positionRepository
            .Setup(r => r.GetByIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Position?)null);

        var dto = new PositionUpdateDto
        {
            Title = "Whatever",
            CareerLevel = CareerLevel.Operational,
            IsCritical = false,
            IsActive = true
        };

        // Act
        Task Act() => _sut.UpdateAsync(1, dto);

        // Assert
        await Assert.ThrowsAsync<NotFoundException>(Act);
    }

    // ===================== DELETE (soft delete) =====================

    [Fact]
    public async Task DeleteAsync_ShouldDeactivatePosition_WhenNoActiveEmployeesAssigned()
    {
        // Arrange
        var position = GetPositionFaker(isActive: true).Generate();

        _positionRepository
            .Setup(r => r.GetByIdAsync(position.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(position);

        _positionRepository
            .Setup(r => r.IsReferencedByActiveEmployeeAsync(position.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        // Act
        await _sut.DeleteAsync(position.Id);

        // Assert
        Assert.False(position.IsActive);

        _auditLogService.Verify(a => a.LogAsync(
            "DELETE_POSITION", nameof(Position), position.Id,
            null, null, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task DeleteAsync_ShouldThrow_WhenReferencedByActiveEmployees()
    {
        // Arrange
        var position = GetPositionFaker(isActive: true).Generate();

        _positionRepository
            .Setup(r => r.GetByIdAsync(position.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(position);

        _positionRepository
            .Setup(r => r.IsReferencedByActiveEmployeeAsync(position.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        Task Act() => _sut.DeleteAsync(position.Id);

        // Assert
        await Assert.ThrowsAsync<DomainException>(Act);
        Assert.True(position.IsActive);
    }

    [Fact]
    public async Task DeleteAsync_ShouldBeIdempotent_WhenAlreadyInactive()
    {
        // Arrange
        var position = GetPositionFaker(isActive: false).Generate();

        _positionRepository
            .Setup(r => r.GetByIdAsync(position.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(position);

        // Act
        await _sut.DeleteAsync(position.Id);

        // Assert
        _positionRepository.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
        _auditLogService.Verify(a => a.LogAsync(
            It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int?>(),
            It.IsAny<object>(), It.IsAny<object>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    // ===================== REACTIVATE =====================

    [Fact]
    public async Task ReactivateAsync_ShouldActivatePosition_WhenCurrentlyInactive()
    {
        // Arrange
        var position = GetPositionFaker(isActive: false).Generate();

        _positionRepository
            .Setup(r => r.GetByIdAsync(position.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(position);

        // Act
        await _sut.ReactivateAsync(position.Id);

        // Assert
        Assert.True(position.IsActive);

        _auditLogService.Verify(a => a.LogAsync(
            "REACTIVATE_POSITION", nameof(Position), position.Id,
            null, null, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task ReactivateAsync_ShouldBeIdempotent_WhenAlreadyActive()
    {
        // Arrange
        var position = GetPositionFaker(isActive: true).Generate();

        _positionRepository
            .Setup(r => r.GetByIdAsync(position.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(position);

        // Act
        await _sut.ReactivateAsync(position.Id);

        // Assert
        _positionRepository.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    // ===================== BULK DELETE / REACTIVATE =====================

    [Fact]
    public async Task DeleteManyAsync_ShouldReportPartialSuccess_WhenSomeFail()
    {
        // Arrange
        var okPosition = GetPositionFaker(isActive: true).Generate();
        okPosition.Id = 1;

        var blockedPosition = GetPositionFaker(isActive: true).Generate();
        blockedPosition.Id = 2;

        const int missingId = 999;

        _positionRepository
            .Setup(r => r.GetByIdAsync(okPosition.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(okPosition);

        _positionRepository
            .Setup(r => r.IsReferencedByActiveEmployeeAsync(okPosition.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        _positionRepository
            .Setup(r => r.GetByIdAsync(blockedPosition.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(blockedPosition);

        _positionRepository
            .Setup(r => r.IsReferencedByActiveEmployeeAsync(blockedPosition.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        _positionRepository
            .Setup(r => r.GetByIdAsync(missingId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Position?)null);

        var dto = new BulkIdsDto { Ids = [okPosition.Id, blockedPosition.Id, missingId] };

        // Act
        var result = await _sut.DeleteManyAsync(dto);

        // Assert
        Assert.Single(result.SucceededIds);
        Assert.Contains(okPosition.Id, result.SucceededIds);
        Assert.Equal(2, result.Failed.Count);
        Assert.False(okPosition.IsActive);
        Assert.True(blockedPosition.IsActive);
    }

    [Fact]
    public async Task ReactivateManyAsync_ShouldReportPartialSuccess_WhenSomeNotFound()
    {
        // Arrange
        var position = GetPositionFaker(isActive: false).Generate();
        const int missingId = 999;

        _positionRepository
            .Setup(r => r.GetByIdAsync(position.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(position);

        _positionRepository
            .Setup(r => r.GetByIdAsync(missingId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Position?)null);

        var dto = new BulkIdsDto { Ids = [position.Id, missingId] };

        // Act
        var result = await _sut.ReactivateManyAsync(dto);

        // Assert
        Assert.Single(result.SucceededIds);
        Assert.Single(result.Failed);
        Assert.True(position.IsActive);
    }

    // ===================== BOGUS FAKER =====================

    private static Faker<Position> GetPositionFaker(bool isActive = true) => new Faker<Position>()
        .RuleFor(p => p.Id, f => f.IndexFaker + 1)
        .RuleFor(p => p.Title, f => f.Name.JobTitle())
        .RuleFor(p => p.CareerLevel, f => f.PickRandom<CareerLevel>())
        .RuleFor(p => p.IsCritical, f => f.Random.Bool())
        .RuleFor(p => p.IsActive, isActive);
}