using Bogus;
using HeimReport.Api.DTOs.Common;
using HeimReport.Api.DTOs.Countries;
using HeimReport.Api.Entities;
using HeimReport.Api.Exceptions;
using HeimReport.Api.Repositories.Countries;
using HeimReport.Api.Services.AuditLogs;
using HeimReport.Api.Services.Countries;
using MockQueryable;
using Moq;

namespace HeimReport.Api.UnitTests.Services.Countries;

public class CountryServiceTests
{
    private readonly Mock<ICountryRepository> _countryRepository = new();
    private readonly Mock<IAuditLogService> _auditLogService = new();

    private readonly CountryService _sut;

    public CountryServiceTests()
    {
        _auditLogService
            .Setup(a => a.LogAsync(
                It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int?>(),
                It.IsAny<object>(), It.IsAny<object>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _sut = new CountryService(_countryRepository.Object, _auditLogService.Object);
    }

    // ===================== GET ACTIVE / INACTIVE PAGED =====================

    [Fact]
    public async Task GetActivePagedAsync_ShouldReturnOnlyActiveCountries()
    {
        // Arrange
        var countries = new List<Country>
        {
            GetCountryFaker(isActive: true).Generate(),
            GetCountryFaker(isActive: true).Generate(),
            GetCountryFaker(isActive: false).Generate()
        };

        _countryRepository.Setup(r => r.Query()).Returns(countries.BuildMock());

        var query = new PaginationQueryDto { PageNumber = 1, PageSize = 10 };

        // Act
        var result = await _sut.GetActivePagedAsync(query);

        // Assert
        Assert.Equal(2, result.TotalCount);
        Assert.All(result.Items, c => Assert.True(c.IsActive));
    }

    [Fact]
    public async Task GetInactivePagedAsync_ShouldReturnOnlyInactiveCountries()
    {
        // Arrange
        var countries = new List<Country>
        {
            GetCountryFaker(isActive: true).Generate(),
            GetCountryFaker(isActive: false).Generate(),
            GetCountryFaker(isActive: false).Generate()
        };

        _countryRepository.Setup(r => r.Query()).Returns(countries.BuildMock());

        var query = new PaginationQueryDto { PageNumber = 1, PageSize = 10 };

        // Act
        var result = await _sut.GetInactivePagedAsync(query);

        // Assert
        Assert.Equal(2, result.TotalCount);
        Assert.All(result.Items, c => Assert.False(c.IsActive));
    }

    // ===================== GET BY ID =====================

    [Fact]
    public async Task GetByIdAsync_ShouldReturnCountry_WhenFound()
    {
        // Arrange
        var country = GetCountryFaker().Generate();

        _countryRepository
            .Setup(r => r.GetByIdAsync(country.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(country);

        // Act
        var result = await _sut.GetByIdAsync(country.Id);

        // Assert
        Assert.Equal(country.Id, result.Id);
        Assert.Equal(country.Name, result.Name);
    }

    [Fact]
    public async Task GetByIdAsync_ShouldThrow_WhenNotFound()
    {
        // Arrange
        _countryRepository
            .Setup(r => r.GetByIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Country?)null);

        // Act
        Task Act() => _sut.GetByIdAsync(1);

        // Assert
        await Assert.ThrowsAsync<NotFoundException>(Act);
    }

    // ===================== CREATE =====================

    [Fact]
    public async Task CreateAsync_ShouldAddCountry_AndLogAudit()
    {
        // Arrange
        var dto = new CountryCreateDto { Name = "Testland", IsActive = true };

        // Act
        var result = await _sut.CreateAsync(dto);

        // Assert
        Assert.Equal(dto.Name, result.Name);
        Assert.True(result.IsActive);

        _countryRepository.Verify(r => r.AddAsync(
            It.Is<Country>(c => c.Name == dto.Name && c.IsActive),
            It.IsAny<CancellationToken>()),
            Times.Once);

        _countryRepository.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);

        _auditLogService.Verify(a => a.LogAsync(
            "CREATE_COUNTRY", nameof(Country), It.IsAny<int?>(),
            null, It.IsAny<object>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    // ===================== UPDATE =====================

    [Fact]
    public async Task UpdateAsync_ShouldUpdateFields_WhenNotDeactivating()
    {
        // Arrange
        var country = GetCountryFaker(isActive: true).Generate();
        var dto = new CountryUpdateDto { Name = "Updated Name", IsActive = true };

        _countryRepository
            .Setup(r => r.GetByIdAsync(country.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(country);

        // Act
        await _sut.UpdateAsync(country.Id, dto);

        // Assert
        Assert.Equal(dto.Name, country.Name);

        _countryRepository.Verify(r => r.Update(country), Times.Once);
        _countryRepository.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);

        _auditLogService.Verify(a => a.LogAsync(
            "UPDATE_COUNTRY", nameof(Country), country.Id,
            It.IsAny<object>(), It.IsAny<object>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_ShouldThrow_WhenDeactivatingWithActiveEmployeesAssigned()
    {
        // Arrange
        var country = GetCountryFaker(isActive: true).Generate();
        var dto = new CountryUpdateDto { Name = country.Name, IsActive = false };

        _countryRepository
            .Setup(r => r.GetByIdAsync(country.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(country);

        _countryRepository
            .Setup(r => r.IsReferencedByActiveEmployeeAsync(country.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        Task Act() => _sut.UpdateAsync(country.Id, dto);

        // Assert
        await Assert.ThrowsAsync<DomainException>(Act);
        _countryRepository.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task UpdateAsync_ShouldThrow_WhenCountryNotFound()
    {
        // Arrange
        _countryRepository
            .Setup(r => r.GetByIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Country?)null);

        var dto = new CountryUpdateDto { Name = "Whatever", IsActive = true };

        // Act
        Task Act() => _sut.UpdateAsync(1, dto);

        // Assert
        await Assert.ThrowsAsync<NotFoundException>(Act);
    }

    // ===================== DELETE (soft delete) =====================

    [Fact]
    public async Task DeleteAsync_ShouldDeactivateCountry_WhenNoActiveEmployeesAssigned()
    {
        // Arrange
        var country = GetCountryFaker(isActive: true).Generate();

        _countryRepository
            .Setup(r => r.GetByIdAsync(country.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(country);

        _countryRepository
            .Setup(r => r.IsReferencedByActiveEmployeeAsync(country.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        // Act
        await _sut.DeleteAsync(country.Id);

        // Assert
        Assert.False(country.IsActive);

        _auditLogService.Verify(a => a.LogAsync(
            "DELETE_COUNTRY", nameof(Country), country.Id,
            null, null, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task DeleteAsync_ShouldThrow_WhenReferencedByActiveEmployees()
    {
        // Arrange
        var country = GetCountryFaker(isActive: true).Generate();

        _countryRepository
            .Setup(r => r.GetByIdAsync(country.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(country);

        _countryRepository
            .Setup(r => r.IsReferencedByActiveEmployeeAsync(country.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        Task Act() => _sut.DeleteAsync(country.Id);

        // Assert
        await Assert.ThrowsAsync<DomainException>(Act);
        Assert.True(country.IsActive);
    }

    [Fact]
    public async Task DeleteAsync_ShouldBeIdempotent_WhenAlreadyInactive()
    {
        // Arrange
        var country = GetCountryFaker(isActive: false).Generate();

        _countryRepository
            .Setup(r => r.GetByIdAsync(country.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(country);

        // Act
        await _sut.DeleteAsync(country.Id);

        // Assert
        _countryRepository.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
        _auditLogService.Verify(a => a.LogAsync(
            It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int?>(),
            It.IsAny<object>(), It.IsAny<object>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    // ===================== REACTIVATE =====================

    [Fact]
    public async Task ReactivateAsync_ShouldActivateCountry_WhenCurrentlyInactive()
    {
        // Arrange
        var country = GetCountryFaker(isActive: false).Generate();

        _countryRepository
            .Setup(r => r.GetByIdAsync(country.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(country);

        // Act
        await _sut.ReactivateAsync(country.Id);

        // Assert
        Assert.True(country.IsActive);

        _auditLogService.Verify(a => a.LogAsync(
            "REACTIVATE_COUNTRY", nameof(Country), country.Id,
            null, null, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task ReactivateAsync_ShouldBeIdempotent_WhenAlreadyActive()
    {
        // Arrange
        var country = GetCountryFaker(isActive: true).Generate();

        _countryRepository
            .Setup(r => r.GetByIdAsync(country.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(country);

        // Act
        await _sut.ReactivateAsync(country.Id);

        // Assert
        _countryRepository.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    // ===================== BULK DELETE / REACTIVATE =====================

    [Fact]
    public async Task DeleteManyAsync_ShouldReportPartialSuccess_WhenSomeFail()
    {
        // Arrange
        var okCountry = GetCountryFaker(isActive: true).Generate();
        okCountry.Id = 1;

        var blockedCountry = GetCountryFaker(isActive: true).Generate();
        blockedCountry.Id = 2;

        const int missingId = 999;

        _countryRepository
            .Setup(r => r.GetByIdAsync(okCountry.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(okCountry);

        _countryRepository
            .Setup(r => r.IsReferencedByActiveEmployeeAsync(okCountry.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        _countryRepository
            .Setup(r => r.GetByIdAsync(blockedCountry.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(blockedCountry);

        _countryRepository
            .Setup(r => r.IsReferencedByActiveEmployeeAsync(blockedCountry.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        _countryRepository
            .Setup(r => r.GetByIdAsync(missingId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Country?)null);

        var dto = new BulkIdsDto { Ids = [okCountry.Id, blockedCountry.Id, missingId] };

        // Act
        var result = await _sut.DeleteManyAsync(dto);

        // Assert
        Assert.Single(result.SucceededIds);
        Assert.Contains(okCountry.Id, result.SucceededIds);
        Assert.Equal(2, result.Failed.Count);
        Assert.False(okCountry.IsActive);
        Assert.True(blockedCountry.IsActive);
    }

    [Fact]
    public async Task ReactivateManyAsync_ShouldReportPartialSuccess_WhenSomeNotFound()
    {
        // Arrange
        var country = GetCountryFaker(isActive: false).Generate();
        const int missingId = 999;

        _countryRepository
            .Setup(r => r.GetByIdAsync(country.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(country);

        _countryRepository
            .Setup(r => r.GetByIdAsync(missingId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Country?)null);

        var dto = new BulkIdsDto { Ids = [country.Id, missingId] };

        // Act
        var result = await _sut.ReactivateManyAsync(dto);

        // Assert
        Assert.Single(result.SucceededIds);
        Assert.Single(result.Failed);
        Assert.True(country.IsActive);
    }

    // ===================== BOGUS FAKER =====================

    private static Faker<Country> GetCountryFaker(bool isActive = true) => new Faker<Country>()
        .RuleFor(c => c.Id, f => f.IndexFaker + 1)
        .RuleFor(c => c.Name, f => f.Address.Country())
        .RuleFor(c => c.IsActive, isActive);
}