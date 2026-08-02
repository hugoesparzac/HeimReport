using FluentValidation;
using FluentValidation.TestHelper;
using HeimReport.Api.DTOs.Departments;
using HeimReport.Api.Repositories.Departments;
using HeimReport.Api.Validators.Departments;
using Moq;

namespace HeimReport.Api.UnitTests.Validators.Departments;

public class DepartmentUpdateDtoValidatorTests
{
    private readonly Mock<IDepartmentRepository> _departmentRepository = new();
    private readonly DepartmentUpdateDtoValidator _sut;

    private const int DepartmentId = 5;

    public DepartmentUpdateDtoValidatorTests()
    {
        _departmentRepository
            .Setup(r => r.ExistsByNameAsync(It.IsAny<string>(), It.IsAny<int?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        _sut = new DepartmentUpdateDtoValidator(_departmentRepository.Object);
    }

    private static ValidationContext<DepartmentUpdateDto> BuildContext(DepartmentUpdateDto dto, int? departmentId = DepartmentId)
    {
        var context = new ValidationContext<DepartmentUpdateDto>(dto);
        if (departmentId.HasValue)
        {
            context.RootContextData["DepartmentId"] = departmentId.Value;
        }
        return context;
    }

    [Fact]
    public async Task ShouldNotHaveError_WhenNameAlreadyExists_ExcludingItself()
    {
        // Arrange
        _departmentRepository
            .Setup(r => r.ExistsByNameAsync("Human Resources", DepartmentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var dto = new DepartmentUpdateDto { Name = "Human Resources", IsActive = true };
        var context = BuildContext(dto);

        // Act
        var result = await _sut.TestValidateAsync(context);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.Name);

        _departmentRepository.Verify(
            r => r.ExistsByNameAsync("Human Resources", DepartmentId, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ShouldHaveError_WhenNameBelongsToAnotherDepartment()
    {
        // Arrange
        _departmentRepository
            .Setup(r => r.ExistsByNameAsync("Human Resources", DepartmentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var dto = new DepartmentUpdateDto { Name = "Human Resources", IsActive = true };
        var context = BuildContext(dto);

        // Act
        var result = await _sut.TestValidateAsync(context);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Name)
            .WithErrorMessage("A department with this name already exists");
    }

    [Fact]
    public async Task ShouldThrow_WhenDepartmentIdIsMissingFromRootContextData()
    {
        // Arrange
        var dto = new DepartmentUpdateDto { Name = "Human Resources", IsActive = true };
        var context = BuildContext(dto, departmentId: null);

        // Act
        Task Act() => _sut.ValidateAsync(context);

        // Assert
        await Assert.ThrowsAsync<InvalidOperationException>(Act);
    }
}