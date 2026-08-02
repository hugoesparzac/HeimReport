using FluentValidation.TestHelper;
using HeimReport.Api.DTOs.Departments;
using HeimReport.Api.Repositories.Departments;
using HeimReport.Api.Validators.Departments;
using Moq;

namespace HeimReport.Api.UnitTests.Validators.Departments;

public class DepartmentCreateDtoValidatorTests
{
    private readonly Mock<IDepartmentRepository> _departmentRepository = new();
    private readonly DepartmentCreateDtoValidator _sut;

    public DepartmentCreateDtoValidatorTests()
    {
        _departmentRepository
            .Setup(r => r.ExistsByNameAsync(It.IsAny<string>(), It.IsAny<int?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        _sut = new DepartmentCreateDtoValidator(_departmentRepository.Object);
    }

    [Fact]
    public async Task ShouldHaveError_WhenNameIsEmpty()
    {
        // Arrange
        var dto = new DepartmentCreateDto { Name = "" };

        // Act
        var result = await _sut.TestValidateAsync(dto);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Name)
            .WithErrorMessage("Name is required");
    }

    [Fact]
    public async Task ShouldHaveError_WhenNameExceedsMaxLength()
    {
        // Arrange
        var dto = new DepartmentCreateDto { Name = new string('A', 101) };

        // Act
        var result = await _sut.TestValidateAsync(dto);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Name)
            .WithErrorMessage("Name cannot exceed 100 characters");
    }

    [Fact]
    public async Task ShouldHaveError_WhenNameAlreadyExists()
    {
        // Arrange
        _departmentRepository
            .Setup(r => r.ExistsByNameAsync("Human Resources", null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var dto = new DepartmentCreateDto { Name = "Human Resources" };

        // Act
        var result = await _sut.TestValidateAsync(dto);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Name)
            .WithErrorMessage("A department with this name already exists");
    }

    [Fact]
    public async Task ShouldNotHaveAnyErrors_WhenDtoIsValid()
    {
        // Arrange
        var dto = new DepartmentCreateDto { Name = "Human Resources" };

        // Act
        var result = await _sut.TestValidateAsync(dto);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }
}