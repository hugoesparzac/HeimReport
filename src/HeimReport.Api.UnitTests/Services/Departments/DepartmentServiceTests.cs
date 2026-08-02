using Bogus;
using HeimReport.Api.DTOs.Common;
using HeimReport.Api.DTOs.Departments;
using HeimReport.Api.Entities;
using HeimReport.Api.Exceptions;
using HeimReport.Api.Repositories.Departments;
using HeimReport.Api.Services.AuditLogs;
using HeimReport.Api.Services.Departments;
using MockQueryable;
using Moq;

namespace HeimReport.Api.UnitTests.Services.Departments;

public class DepartmentServiceTests
{
    private readonly Mock<IDepartmentRepository> _departmentRepository = new();
    private readonly Mock<IAuditLogService> _auditLogService = new();

    private readonly DepartmentService _sut;

    public DepartmentServiceTests()
    {
        _auditLogService
            .Setup(a => a.LogAsync(
                It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int?>(),
                It.IsAny<object>(), It.IsAny<object>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _sut = new DepartmentService(_departmentRepository.Object, _auditLogService.Object);
    }

    // ===================== GET ACTIVE / INACTIVE PAGED =====================

    [Fact]
    public async Task GetActivePagedAsync_ShouldReturnOnlyActiveDepartments()
    {
        // Arrange
        var departments = new List<Department>
        {
            GetDepartmentFaker(isActive: true).Generate(),
            GetDepartmentFaker(isActive: true).Generate(),
            GetDepartmentFaker(isActive: false).Generate()
        };

        _departmentRepository.Setup(r => r.Query()).Returns(departments.BuildMock());

        var query = new PaginationQueryDto { PageNumber = 1, PageSize = 10 };

        // Act
        var result = await _sut.GetActivePagedAsync(query);

        // Assert
        Assert.Equal(2, result.TotalCount);
        Assert.All(result.Items, d => Assert.True(d.IsActive));
    }

    [Fact]
    public async Task GetInactivePagedAsync_ShouldReturnOnlyInactiveDepartments()
    {
        // Arrange
        var departments = new List<Department>
        {
            GetDepartmentFaker(isActive: true).Generate(),
            GetDepartmentFaker(isActive: false).Generate(),
            GetDepartmentFaker(isActive: false).Generate()
        };

        _departmentRepository.Setup(r => r.Query()).Returns(departments.BuildMock());

        var query = new PaginationQueryDto { PageNumber = 1, PageSize = 10 };

        // Act
        var result = await _sut.GetInactivePagedAsync(query);

        // Assert
        Assert.Equal(2, result.TotalCount);
        Assert.All(result.Items, d => Assert.False(d.IsActive));
    }

    // ===================== GET BY ID =====================

    [Fact]
    public async Task GetByIdAsync_ShouldReturnDepartment_WhenFound()
    {
        // Arrange
        var department = GetDepartmentFaker().Generate();

        _departmentRepository
            .Setup(r => r.GetByIdAsync(department.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(department);

        // Act
        var result = await _sut.GetByIdAsync(department.Id);

        // Assert
        Assert.Equal(department.Id, result.Id);
        Assert.Equal(department.Name, result.Name);
    }

    [Fact]
    public async Task GetByIdAsync_ShouldThrow_WhenNotFound()
    {
        // Arrange
        _departmentRepository
            .Setup(r => r.GetByIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Department?)null);

        // Act
        Task Act() => _sut.GetByIdAsync(1);

        // Assert
        await Assert.ThrowsAsync<NotFoundException>(Act);
    }

    // ===================== CREATE =====================

    [Fact]
    public async Task CreateAsync_ShouldAddDepartment_AndLogAudit()
    {
        // Arrange
        var dto = new DepartmentCreateDto { Name = "Quality Assurance", IsActive = true };

        // Act
        var result = await _sut.CreateAsync(dto);

        // Assert
        Assert.Equal(dto.Name, result.Name);
        Assert.True(result.IsActive);

        _departmentRepository.Verify(r => r.AddAsync(
            It.Is<Department>(d => d.Name == dto.Name && d.IsActive),
            It.IsAny<CancellationToken>()),
            Times.Once);

        _departmentRepository.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);

        _auditLogService.Verify(a => a.LogAsync(
            "CREATE_DEPARTMENT", nameof(Department), It.IsAny<int?>(),
            null, It.IsAny<object>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    // ===================== UPDATE =====================

    [Fact]
    public async Task UpdateAsync_ShouldUpdateFields_WhenNotDeactivating()
    {
        // Arrange
        var department = GetDepartmentFaker(isActive: true).Generate();
        var dto = new DepartmentUpdateDto { Name = "Updated Department", IsActive = true };

        _departmentRepository
            .Setup(r => r.GetByIdAsync(department.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(department);

        // Act
        await _sut.UpdateAsync(department.Id, dto);

        // Assert
        Assert.Equal(dto.Name, department.Name);

        _departmentRepository.Verify(r => r.Update(department), Times.Once);
        _departmentRepository.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);

        _auditLogService.Verify(a => a.LogAsync(
            "UPDATE_DEPARTMENT", nameof(Department), department.Id,
            It.IsAny<object>(), It.IsAny<object>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_ShouldThrow_WhenDeactivatingWithActiveEmployeesAssigned()
    {
        // Arrange
        var department = GetDepartmentFaker(isActive: true).Generate();
        var dto = new DepartmentUpdateDto { Name = department.Name, IsActive = false };

        _departmentRepository
            .Setup(r => r.GetByIdAsync(department.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(department);

        _departmentRepository
            .Setup(r => r.IsReferencedByActiveEmployeeAsync(department.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        Task Act() => _sut.UpdateAsync(department.Id, dto);

        // Assert
        await Assert.ThrowsAsync<DomainException>(Act);
        _departmentRepository.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task UpdateAsync_ShouldThrow_WhenDepartmentNotFound()
    {
        // Arrange
        _departmentRepository
            .Setup(r => r.GetByIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Department?)null);

        var dto = new DepartmentUpdateDto { Name = "Whatever", IsActive = true };

        // Act
        Task Act() => _sut.UpdateAsync(1, dto);

        // Assert
        await Assert.ThrowsAsync<NotFoundException>(Act);
    }

    // ===================== DELETE (soft delete) =====================

    [Fact]
    public async Task DeleteAsync_ShouldDeactivateDepartment_WhenNoActiveEmployeesAssigned()
    {
        // Arrange
        var department = GetDepartmentFaker(isActive: true).Generate();

        _departmentRepository
            .Setup(r => r.GetByIdAsync(department.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(department);

        _departmentRepository
            .Setup(r => r.IsReferencedByActiveEmployeeAsync(department.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        // Act
        await _sut.DeleteAsync(department.Id);

        // Assert
        Assert.False(department.IsActive);

        _auditLogService.Verify(a => a.LogAsync(
            "DELETE_DEPARTMENT", nameof(Department), department.Id,
            null, null, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task DeleteAsync_ShouldThrow_WhenReferencedByActiveEmployees()
    {
        // Arrange
        var department = GetDepartmentFaker(isActive: true).Generate();

        _departmentRepository
            .Setup(r => r.GetByIdAsync(department.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(department);

        _departmentRepository
            .Setup(r => r.IsReferencedByActiveEmployeeAsync(department.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        Task Act() => _sut.DeleteAsync(department.Id);

        // Assert
        await Assert.ThrowsAsync<DomainException>(Act);
        Assert.True(department.IsActive);
    }

    [Fact]
    public async Task DeleteAsync_ShouldBeIdempotent_WhenAlreadyInactive()
    {
        // Arrange
        var department = GetDepartmentFaker(isActive: false).Generate();

        _departmentRepository
            .Setup(r => r.GetByIdAsync(department.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(department);

        // Act
        await _sut.DeleteAsync(department.Id);

        // Assert
        _departmentRepository.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
        _auditLogService.Verify(a => a.LogAsync(
            It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int?>(),
            It.IsAny<object>(), It.IsAny<object>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    // ===================== REACTIVATE =====================

    [Fact]
    public async Task ReactivateAsync_ShouldActivateDepartment_WhenCurrentlyInactive()
    {
        // Arrange
        var department = GetDepartmentFaker(isActive: false).Generate();

        _departmentRepository
            .Setup(r => r.GetByIdAsync(department.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(department);

        // Act
        await _sut.ReactivateAsync(department.Id);

        // Assert
        Assert.True(department.IsActive);

        _auditLogService.Verify(a => a.LogAsync(
            "REACTIVATE_DEPARTMENT", nameof(Department), department.Id,
            null, null, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task ReactivateAsync_ShouldBeIdempotent_WhenAlreadyActive()
    {
        // Arrange
        var department = GetDepartmentFaker(isActive: true).Generate();

        _departmentRepository
            .Setup(r => r.GetByIdAsync(department.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(department);

        // Act
        await _sut.ReactivateAsync(department.Id);

        // Assert
        _departmentRepository.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    // ===================== BULK DELETE / REACTIVATE =====================

    [Fact]
    public async Task DeleteManyAsync_ShouldReportPartialSuccess_WhenSomeFail()
    {
        // Arrange
        var okDepartment = GetDepartmentFaker(isActive: true).Generate();
        okDepartment.Id = 1;

        var blockedDepartment = GetDepartmentFaker(isActive: true).Generate();
        blockedDepartment.Id = 2;

        const int missingId = 999;

        _departmentRepository
            .Setup(r => r.GetByIdAsync(okDepartment.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(okDepartment);

        _departmentRepository
            .Setup(r => r.IsReferencedByActiveEmployeeAsync(okDepartment.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        _departmentRepository
            .Setup(r => r.GetByIdAsync(blockedDepartment.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(blockedDepartment);

        _departmentRepository
            .Setup(r => r.IsReferencedByActiveEmployeeAsync(blockedDepartment.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        _departmentRepository
            .Setup(r => r.GetByIdAsync(missingId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Department?)null);

        var dto = new BulkIdsDto { Ids = [okDepartment.Id, blockedDepartment.Id, missingId] };

        // Act
        var result = await _sut.DeleteManyAsync(dto);

        // Assert
        Assert.Single(result.SucceededIds);
        Assert.Contains(okDepartment.Id, result.SucceededIds);
        Assert.Equal(2, result.Failed.Count);
        Assert.False(okDepartment.IsActive);
        Assert.True(blockedDepartment.IsActive);
    }

    [Fact]
    public async Task ReactivateManyAsync_ShouldReportPartialSuccess_WhenSomeNotFound()
    {
        // Arrange
        var department = GetDepartmentFaker(isActive: false).Generate();
        const int missingId = 999;

        _departmentRepository
            .Setup(r => r.GetByIdAsync(department.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(department);

        _departmentRepository
            .Setup(r => r.GetByIdAsync(missingId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Department?)null);

        var dto = new BulkIdsDto { Ids = [department.Id, missingId] };

        // Act
        var result = await _sut.ReactivateManyAsync(dto);

        // Assert
        Assert.Single(result.SucceededIds);
        Assert.Single(result.Failed);
        Assert.True(department.IsActive);
    }

    // ===================== BOGUS FAKER =====================

    private static Faker<Department> GetDepartmentFaker(bool isActive = true) => new Faker<Department>()
        .RuleFor(d => d.Id, f => f.IndexFaker + 1)
        .RuleFor(d => d.Name, f => f.Commerce.Department())
        .RuleFor(d => d.IsActive, isActive);
}