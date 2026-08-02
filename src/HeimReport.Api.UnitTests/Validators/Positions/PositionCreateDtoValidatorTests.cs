using FluentValidation.TestHelper;
using HeimReport.Api.DTOs.Positions;
using HeimReport.Api.Enums;
using HeimReport.Api.Repositories.Positions;
using HeimReport.Api.Validators.Positions;
using Moq;

namespace HeimReport.Api.UnitTests.Validators.Positions;

public class PositionCreateDtoValidatorTests
{
    private readonly Mock<IPositionRepository> _positionRepository = new();
    private readonly PositionCreateDtoValidator _sut;

    public PositionCreateDtoValidatorTests()
    {
        _positionRepository
            .Setup(r => r.ExistsByTitleAsync(It.IsAny<string>(), It.IsAny<int?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        _sut = new PositionCreateDtoValidator(_positionRepository.Object);
    }

    [Fact]
    public async Task ShouldHaveError_WhenTitleIsEmpty()
    {
        // Arrange
        var dto = new PositionCreateDto { Title = "", CareerLevel = CareerLevel.Professional, IsCritical = false };

        // Act
        var result = await _sut.TestValidateAsync(dto);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Title);
    }

    [Fact]
    public async Task ShouldHaveError_WhenTitleExceedsMaxLength()
    {
        // Arrange
        var dto = new PositionCreateDto { Title = new string('A', 101), CareerLevel = CareerLevel.Professional, IsCritical = false };

        // Act
        var result = await _sut.TestValidateAsync(dto);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Title)
            .WithErrorMessage("Title cannot exceed 100 characters");
    }

    [Fact]
    public async Task ShouldHaveError_WhenTitleAlreadyExists()
    {
        // Arrange
        _positionRepository
            .Setup(r => r.ExistsByTitleAsync("Analyst", null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var dto = new PositionCreateDto { Title = "Analyst", CareerLevel = CareerLevel.Professional, IsCritical = false };

        // Act
        var result = await _sut.TestValidateAsync(dto);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Title)
            .WithErrorMessage("A position with this title already exists");
    }

    [Fact]
    public async Task ShouldHaveError_WhenCareerLevelIsInvalidEnumValue()
    {
        // Arrange
        var dto = new PositionCreateDto { Title = "Analyst", CareerLevel = (CareerLevel)999, IsCritical = false };

        // Act
        var result = await _sut.TestValidateAsync(dto);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.CareerLevel)
            .WithErrorMessage("Invalid Career Level");
    }

    [Fact]
    public async Task ShouldNotHaveAnyErrors_WhenDtoIsValid()
    {
        // Arrange
        var dto = new PositionCreateDto { Title = "Analyst", CareerLevel = CareerLevel.Professional, IsCritical = false };

        // Act
        var result = await _sut.TestValidateAsync(dto);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }
}