using Bogus;
using HeimReport.Api.DTOs.Employees;
using HeimReport.Api.Entities;
using HeimReport.Api.Enums;
using HeimReport.Api.Exceptions;
using HeimReport.Api.Repositories.Employees;
using HeimReport.Api.Services.AuditLogs;
using HeimReport.Api.Services.Employees;
using HeimReport.Api.Storage;
using Microsoft.AspNetCore.Http;
using MockQueryable;
using Moq;

namespace HeimReport.Api.UnitTests.Services.Employees;

public class EmployeeServiceTests
{
    private readonly Mock<IEmployeeRepository> _employeeRepository = new();
    private readonly Mock<IEmployeeJobHistoryRepository> _jobHistoryRepository = new();
    private readonly Mock<IAuditLogService> _auditLogService = new();
    private readonly Mock<IPhotoStorageService> _photoStorageService = new();

    private readonly EmployeeService _sut;

    public EmployeeServiceTests()
    {
        _auditLogService
            .Setup(a => a.LogAsync(
                It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int?>(),
                It.IsAny<object>(), It.IsAny<object>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _sut = new EmployeeService(
            _employeeRepository.Object,
            _jobHistoryRepository.Object,
            _auditLogService.Object,
            _photoStorageService.Object);
    }

    // ===================== GET PAGED =====================

    [Fact]
    public async Task GetPagedAsync_ShouldFilterByStatus()
    {
        // Arrange
        var active1 = WithNavigationProperties(GetEmployeeFaker(status: EmployeeStatus.Active).Generate());
        var active2 = WithNavigationProperties(GetEmployeeFaker(status: EmployeeStatus.Active).Generate());
        var terminated = WithNavigationProperties(GetEmployeeFaker(status: EmployeeStatus.InvoluntaryTermination).Generate());

        var employees = new List<Employee> { active1, active2, terminated };

        _employeeRepository.Setup(r => r.QueryWithDetails()).Returns(employees.BuildMock());

        var query = new EmployeeQueryDto { PageNumber = 1, PageSize = 10, Status = EmployeeStatus.Active };

        // Act
        var result = await _sut.GetPagedAsync(query);

        // Assert
        Assert.Equal(2, result.TotalCount);
    }

    // ===================== GET BY ID =====================

    [Fact]
    public async Task GetByIdAsync_ShouldReturnEmployee_WhenFound()
    {
        // Arrange
        var employee = WithNavigationProperties(GetEmployeeFaker().Generate());

        _employeeRepository
            .Setup(r => r.GetByIdWithDetailsAsync(employee.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(employee);

        // Act
        var result = await _sut.GetByIdAsync(employee.Id);

        // Assert
        Assert.Equal(employee.Id, result.Id);
    }

    [Fact]
    public async Task GetByIdAsync_ShouldThrow_WhenNotFound()
    {
        // Arrange
        _employeeRepository
            .Setup(r => r.GetByIdWithDetailsAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Employee?)null);

        // Act
        Task Act() => _sut.GetByIdAsync(1);

        // Assert
        await Assert.ThrowsAsync<NotFoundException>(Act);
    }

    // ===================== CREATE =====================

    [Fact]
    public async Task CreateAsync_ShouldAddEmployee_CreateInitialJobHistory_AndLogAudit()
    {
        // Arrange
        var dto = GetCreateDtoFaker().Generate();

        Employee? capturedEmployee = null;
        _employeeRepository
            .Setup(r => r.AddAsync(It.IsAny<Employee>(), It.IsAny<CancellationToken>()))
            .Callback<Employee, CancellationToken>((e, _) =>
            {
                e.Id = 42;
                capturedEmployee = e;
            })
            .Returns(Task.CompletedTask);

        var createdEmployee = WithNavigationProperties(GetEmployeeFaker().Generate());
        createdEmployee.Id = 42;

        _employeeRepository
            .Setup(r => r.GetByIdWithDetailsAsync(42, It.IsAny<CancellationToken>()))
            .ReturnsAsync(createdEmployee);

        // Act
        var result = await _sut.CreateAsync(dto);

        // Assert
        Assert.NotNull(capturedEmployee);
        Assert.Equal(dto.FirstName, capturedEmployee!.FirstName);
        Assert.Equal(EmployeeStatus.Active, capturedEmployee.Status);

        _jobHistoryRepository.Verify(r => r.AddAsync(
            It.Is<EmployeeJobHistory>(h =>
                h.EmployeeId == 42 &&
                h.DepartmentId == dto.DepartmentId &&
                h.ChangeReason == null),
            It.IsAny<CancellationToken>()),
            Times.Once);

        _auditLogService.Verify(a => a.LogAsync(
            "CREATE_EMPLOYEE", nameof(Employee), 42,
            null, It.IsAny<object>(), It.IsAny<CancellationToken>()),
            Times.Once);

        Assert.Equal(42, result.Id);
    }

    // ===================== UPDATE — cambio normal de puesto =====================

    [Fact]
    public async Task UpdateAsync_ShouldCloseAndOpenJobHistory_WhenJobFieldsChange()
    {
        // Arrange
        var employee = GetEmployeeFaker(status: EmployeeStatus.Active).Generate();
        var dto = GetUpdateDtoFaker(status: EmployeeStatus.Active).Generate();
        dto = dto with { DepartmentId = employee.DepartmentId + 1, ChangeReason = JobChangeReason.Promotion };

        var openHistory = new EmployeeJobHistory
        {
            Id = 1,
            EmployeeId = employee.Id,
            DepartmentId = employee.DepartmentId,
            PositionId = employee.PositionId,
            StartDate = employee.HireDate,
            CreatedAt = DateTime.UtcNow
        };

        _employeeRepository
            .Setup(r => r.GetByIdAsync(employee.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(employee);

        _jobHistoryRepository
            .Setup(r => r.GetOpenRecordAsync(employee.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(openHistory);

        // Act
        await _sut.UpdateAsync(employee.Id, dto);

        // Assert
        Assert.NotNull(openHistory.EndDate); // se cerró el historial anterior

        _jobHistoryRepository.Verify(r => r.AddAsync(
            It.Is<EmployeeJobHistory>(h =>
                h.EmployeeId == employee.Id &&
                h.DepartmentId == dto.DepartmentId &&
                h.ChangeReason == JobChangeReason.Promotion),
            It.IsAny<CancellationToken>()),
            Times.Once);

        _auditLogService.Verify(a => a.LogAsync(
            "UPDATE_EMPLOYEE", nameof(Employee), employee.Id,
            It.IsAny<object>(), It.IsAny<object>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_ShouldNotTouchJobHistory_WhenNoJobFieldsChange()
    {
        // Arrange
        var employee = GetEmployeeFaker(status: EmployeeStatus.Active).Generate();
        var dto = new EmployeeUpdateDto
        {
            FirstName = "UpdatedFirstNameOnly",
            LastName = employee.LastName,
            Email = employee.Email,
            NationalId = employee.NationalId,
            BirthDate = employee.BirthDate,
            ContractType = employee.ContractType,
            Status = EmployeeStatus.Active,
            CurrentSalary = employee.CurrentSalary,
            CountryId = employee.CountryId,
            DepartmentId = employee.DepartmentId,
            PositionId = employee.PositionId,
            ManagerId = employee.ManagerId
        };

        _employeeRepository
            .Setup(r => r.GetByIdAsync(employee.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(employee);

        // Act
        await _sut.UpdateAsync(employee.Id, dto);

        // Assert
        Assert.Equal("UpdatedFirstNameOnly", employee.FirstName);

        _jobHistoryRepository.Verify(r => r.AddAsync(It.IsAny<EmployeeJobHistory>(), It.IsAny<CancellationToken>()), Times.Never);
        _jobHistoryRepository.Verify(r => r.GetOpenRecordAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    // ===================== UPDATE — baja =====================

    [Fact]
    public async Task UpdateAsync_ShouldCloseJobHistoryWithTerminationReason_WhenEmployeeGoesOnLeave()
    {
        // Arrange
        var employee = GetEmployeeFaker(status: EmployeeStatus.Active).Generate();
        var terminationDate = DateTime.UtcNow;

        var dto = new EmployeeUpdateDto
        {
            FirstName = employee.FirstName,
            LastName = employee.LastName,
            Email = employee.Email,
            NationalId = employee.NationalId,
            BirthDate = employee.BirthDate,
            ContractType = employee.ContractType,
            Status = EmployeeStatus.VoluntaryResignation,
            TerminationDate = terminationDate,
            CurrentSalary = employee.CurrentSalary,
            CountryId = employee.CountryId,
            DepartmentId = employee.DepartmentId,
            PositionId = employee.PositionId,
            ManagerId = employee.ManagerId
        };

        var openHistory = new EmployeeJobHistory
        {
            Id = 1,
            EmployeeId = employee.Id,
            DepartmentId = employee.DepartmentId,
            PositionId = employee.PositionId,
            StartDate = employee.HireDate,
            CreatedAt = DateTime.UtcNow
        };

        _employeeRepository
            .Setup(r => r.GetByIdAsync(employee.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(employee);

        _jobHistoryRepository
            .Setup(r => r.GetOpenRecordAsync(employee.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(openHistory);

        // Act
        await _sut.UpdateAsync(employee.Id, dto);

        // Assert
        Assert.Equal(JobChangeReason.Termination, openHistory.ChangeReason);
        Assert.Equal(terminationDate, openHistory.EndDate);

        // No debe abrirse un nuevo registro en una baja
        _jobHistoryRepository.Verify(r => r.AddAsync(It.IsAny<EmployeeJobHistory>(), It.IsAny<CancellationToken>()), Times.Never);

        _auditLogService.Verify(a => a.LogAsync(
            "TERMINATE_EMPLOYEE", nameof(Employee), employee.Id,
            It.IsAny<object>(), It.IsAny<object>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    // ===================== UPDATE — reactivación =====================

    [Fact]
    public async Task UpdateAsync_ShouldOpenNewJobHistoryWithReactivationReason_WhenEmployeeReturnsFromLeave()
    {
        // Arrange
        var employee = GetEmployeeFaker(status: EmployeeStatus.VoluntaryResignation).Generate();

        var dto = new EmployeeUpdateDto
        {
            FirstName = employee.FirstName,
            LastName = employee.LastName,
            Email = employee.Email,
            NationalId = employee.NationalId,
            BirthDate = employee.BirthDate,
            ContractType = employee.ContractType,
            Status = EmployeeStatus.Active,
            CurrentSalary = employee.CurrentSalary,
            CountryId = employee.CountryId,
            DepartmentId = employee.DepartmentId,
            PositionId = employee.PositionId,
            ManagerId = employee.ManagerId
        };

        _employeeRepository
            .Setup(r => r.GetByIdAsync(employee.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(employee);

        _jobHistoryRepository
            .Setup(r => r.GetOpenRecordAsync(employee.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync((EmployeeJobHistory?)null);

        // Act
        await _sut.UpdateAsync(employee.Id, dto);

        // Assert
        _jobHistoryRepository.Verify(r => r.AddAsync(
            It.Is<EmployeeJobHistory>(h =>
                h.EmployeeId == employee.Id &&
                h.ChangeReason == JobChangeReason.Reactivation),
            It.IsAny<CancellationToken>()),
            Times.Once);

        _auditLogService.Verify(a => a.LogAsync(
            "REACTIVATE_EMPLOYEE", nameof(Employee), employee.Id,
            It.IsAny<object>(), It.IsAny<object>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_ShouldThrow_WhenEmployeeNotFound()
    {
        // Arrange
        _employeeRepository
            .Setup(r => r.GetByIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Employee?)null);

        var dto = GetUpdateDtoFaker(status: EmployeeStatus.Active).Generate();

        // Act
        Task Act() => _sut.UpdateAsync(1, dto);

        // Assert
        await Assert.ThrowsAsync<NotFoundException>(Act);
    }

    // ===================== TERMINATE MANY =====================

    [Fact]
    public async Task TerminateManyAsync_ShouldReportPartialSuccess_WhenSomeAreNotActiveOrNotFound()
    {
        // Arrange
        var activeEmployee = GetEmployeeFaker(status: EmployeeStatus.Active).Generate();
        activeEmployee.Id = 1;

        var alreadyTerminated = GetEmployeeFaker(status: EmployeeStatus.InvoluntaryTermination).Generate();
        alreadyTerminated.Id = 2;

        const int missingId = 999;

        _employeeRepository
            .Setup(r => r.GetByIdAsync(activeEmployee.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(activeEmployee);

        _employeeRepository
            .Setup(r => r.GetByIdAsync(alreadyTerminated.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(alreadyTerminated);

        _employeeRepository
            .Setup(r => r.GetByIdAsync(missingId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Employee?)null);

        _jobHistoryRepository
            .Setup(r => r.GetOpenRecordAsync(activeEmployee.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync((EmployeeJobHistory?)null);

        var dto = new EmployeeBulkTerminationDto
        {
            EmployeeIds = [activeEmployee.Id, alreadyTerminated.Id, missingId],
            Status = EmployeeStatus.InvoluntaryTermination
        };

        // Act
        var result = await _sut.TerminateManyAsync(dto);

        // Assert
        Assert.Single(result.SucceededIds);
        Assert.Contains(activeEmployee.Id, result.SucceededIds);
        Assert.Equal(2, result.Failed.Count);
        Assert.Equal(EmployeeStatus.InvoluntaryTermination, activeEmployee.Status);

        _auditLogService.Verify(a => a.LogAsync(
            "TERMINATE_EMPLOYEE", nameof(Employee), activeEmployee.Id,
            It.IsAny<object>(), It.IsAny<object>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    // ===================== PHOTO UPLOAD / REMOVE =====================

    [Fact]
    public async Task UploadPhotoAsync_ShouldUploadAndUpdateEmployee_WhenNoExistingPhoto()
    {
        // Arrange
        var employee = GetEmployeeFaker().Generate();
        employee.PhotoPublicId = null;

        var fileMock = new Mock<IFormFile>();
        fileMock.Setup(f => f.FileName).Returns("photo.jpg");
        fileMock.Setup(f => f.OpenReadStream()).Returns(new MemoryStream([1, 2, 3]));

        _employeeRepository
            .Setup(r => r.GetByIdAsync(employee.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(employee);

        _photoStorageService
            .Setup(p => p.UploadAsync(It.IsAny<Stream>(), "photo.jpg", It.IsAny<CancellationToken>()))
            .ReturnsAsync(new PhotoUploadResult("https://cloudinary.example/photo.jpg", "public-id-123"));

        var updatedEmployee = WithNavigationProperties(GetEmployeeFaker().Generate());
        updatedEmployee.Id = employee.Id;
        updatedEmployee.PhotoUrl = "https://cloudinary.example/photo.jpg";

        _employeeRepository
            .Setup(r => r.GetByIdWithDetailsAsync(employee.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(updatedEmployee);

        // Act
        var result = await _sut.UploadPhotoAsync(employee.Id, fileMock.Object);

        // Assert
        Assert.Equal("https://cloudinary.example/photo.jpg", employee.PhotoUrl);
        Assert.Equal("public-id-123", employee.PhotoPublicId);

        _photoStorageService.Verify(p => p.DeleteAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);

        _auditLogService.Verify(a => a.LogAsync(
            "UPDATE_EMPLOYEE_PHOTO", nameof(Employee), employee.Id,
            It.IsAny<object>(), It.IsAny<object>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task UploadPhotoAsync_ShouldDeleteOldPhoto_WhenEmployeeAlreadyHasOne()
    {
        // Arrange
        var employee = GetEmployeeFaker().Generate();
        employee.PhotoPublicId = "old-public-id";
        employee.PhotoUrl = "https://cloudinary.example/old.jpg";

        var fileMock = new Mock<IFormFile>();
        fileMock.Setup(f => f.FileName).Returns("new-photo.jpg");
        fileMock.Setup(f => f.OpenReadStream()).Returns(new MemoryStream([1, 2, 3]));

        _employeeRepository
            .Setup(r => r.GetByIdAsync(employee.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(employee);

        _photoStorageService
            .Setup(p => p.UploadAsync(It.IsAny<Stream>(), "new-photo.jpg", It.IsAny<CancellationToken>()))
            .ReturnsAsync(new PhotoUploadResult("https://cloudinary.example/new.jpg", "new-public-id"));

        WithNavigationProperties(employee);

        _employeeRepository
            .Setup(r => r.GetByIdWithDetailsAsync(employee.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(employee);

        // Act
        await _sut.UploadPhotoAsync(employee.Id, fileMock.Object);

        // Assert
        _photoStorageService.Verify(p => p.DeleteAsync("old-public-id", It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task RemovePhotoAsync_ShouldDeletePhoto_WhenEmployeeHasOne()
    {
        // Arrange
        var employee = GetEmployeeFaker().Generate();
        employee.PhotoPublicId = "public-id-to-remove";
        employee.PhotoUrl = "https://cloudinary.example/photo.jpg";

        _employeeRepository
            .Setup(r => r.GetByIdAsync(employee.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(employee);

        // Act
        await _sut.RemovePhotoAsync(employee.Id);

        // Assert
        Assert.Null(employee.PhotoUrl);
        Assert.Null(employee.PhotoPublicId);

        _photoStorageService.Verify(p => p.DeleteAsync("public-id-to-remove", It.IsAny<CancellationToken>()), Times.Once);

        _auditLogService.Verify(a => a.LogAsync(
            "REMOVE_EMPLOYEE_PHOTO", nameof(Employee), employee.Id,
            It.IsAny<object>(), null, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task RemovePhotoAsync_ShouldBeIdempotent_WhenEmployeeHasNoPhoto()
    {
        // Arrange
        var employee = GetEmployeeFaker().Generate();
        employee.PhotoPublicId = null;

        _employeeRepository
            .Setup(r => r.GetByIdAsync(employee.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(employee);

        // Act
        await _sut.RemovePhotoAsync(employee.Id);

        // Assert
        _photoStorageService.Verify(p => p.DeleteAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
        _employeeRepository.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    // ===================== BOGUS FAKERS =====================

    private static Faker<Employee> GetEmployeeFaker(
        EmployeeStatus status = EmployeeStatus.Active,
        string? firstName = null) => new Faker<Employee>()
        .RuleFor(e => e.Id, f => f.IndexFaker + 1)
        .RuleFor(e => e.FirstName, f => firstName ?? f.Name.FirstName())
        .RuleFor(e => e.LastName, f => f.Name.LastName())
        .RuleFor(e => e.Email, (f, e) => f.Internet.Email(e.FirstName, e.LastName))
        .RuleFor(e => e.NormalizedEmail, (_, e) => e.Email.ToUpperInvariant())
        .RuleFor(e => e.NationalId, f => f.Random.Replace("###-###-###"))
        .RuleFor(e => e.BirthDate, f => DateTime.SpecifyKind(f.Date.Past(30, DateTime.UtcNow.AddYears(-22)), DateTimeKind.Utc))
        .RuleFor(e => e.HireDate, f => DateTime.SpecifyKind(f.Date.Past(5), DateTimeKind.Utc))
        .RuleFor(e => e.ContractType, f => f.PickRandom<ContractType>())
        .RuleFor(e => e.Status, status)
        .RuleFor(e => e.CurrentSalary, f => f.Random.Decimal(10000, 50000))
        .RuleFor(e => e.CountryId, f => f.Random.Int(1, 10))
        .RuleFor(e => e.DepartmentId, f => f.Random.Int(1, 20))
        .RuleFor(e => e.PositionId, f => f.Random.Int(1, 50))
        .RuleFor(e => e.CreatedAt, f => f.Date.Past(6));

    private static Faker<EmployeeCreateDto> GetCreateDtoFaker() => new Faker<EmployeeCreateDto>()
        .RuleFor(d => d.FirstName, f => f.Name.FirstName())
        .RuleFor(d => d.LastName, f => f.Name.LastName())
        .RuleFor(d => d.Email, f => f.Internet.Email())
        .RuleFor(d => d.NationalId, f => f.Random.Replace("###-###-###"))
        .RuleFor(d => d.BirthDate, f => DateTime.SpecifyKind(f.Date.Past(30, DateTime.UtcNow.AddYears(-22)), DateTimeKind.Utc))
        .RuleFor(d => d.HireDate, f => DateTime.SpecifyKind(f.Date.Recent(), DateTimeKind.Utc))
        .RuleFor(d => d.ContractType, f => f.PickRandom<ContractType>())
        .RuleFor(d => d.CurrentSalary, f => f.Random.Decimal(10000, 50000))
        .RuleFor(d => d.CountryId, f => f.Random.Int(1, 10))
        .RuleFor(d => d.DepartmentId, f => f.Random.Int(1, 20))
        .RuleFor(d => d.PositionId, f => f.Random.Int(1, 50));

    private static Faker<EmployeeUpdateDto> GetUpdateDtoFaker(EmployeeStatus status) => new Faker<EmployeeUpdateDto>()
        .RuleFor(d => d.FirstName, f => f.Name.FirstName())
        .RuleFor(d => d.LastName, f => f.Name.LastName())
        .RuleFor(d => d.Email, f => f.Internet.Email())
        .RuleFor(d => d.NationalId, f => f.Random.Replace("###-###-###"))
        .RuleFor(d => d.BirthDate, f => DateTime.SpecifyKind(f.Date.Past(30, DateTime.UtcNow.AddYears(-22)), DateTimeKind.Utc))
        .RuleFor(d => d.ContractType, f => f.PickRandom<ContractType>())
        .RuleFor(d => d.Status, status)
        .RuleFor(d => d.CurrentSalary, f => f.Random.Decimal(10000, 50000))
        .RuleFor(d => d.CountryId, f => f.Random.Int(1, 10))
        .RuleFor(d => d.DepartmentId, f => f.Random.Int(1, 20))
        .RuleFor(d => d.PositionId, f => f.Random.Int(1, 50));

    // ===================== HELPER =====================

    private static Employee WithNavigationProperties(Employee employee)
    {
        employee.Country = new Country { Id = employee.CountryId, Name = "Testland", IsActive = true };
        employee.Department = new Department { Id = employee.DepartmentId, Name = "Test Department", IsActive = true };
        employee.Position = new Position
        {
            Id = employee.PositionId,
            Title = "Test Position",
            CareerLevel = CareerLevel.Professional,
            IsCritical = false,
            IsActive = true
        };
        return employee;
    }
}