using FluentValidation;
using FluentValidation.TestHelper;
using HeimReport.Api.DTOs.Positions;
using HeimReport.Api.Enums;
using HeimReport.Api.Repositories.Positions;
using HeimReport.Api.Validators.Positions;
using Moq;

namespace HeimReport.Api.UnitTests.Validators.Positions;

public class PositionUpdateDtoValidatorTests
{
    private readonly Mock<IPositionRepository> _positionRepository = new();
    private readonly PositionUpdateDtoValidator _sut;

    private const int PositionId = 5;

    public PositionUpdateDtoValidatorTests()
    {
        _positionRepository
            .Setup(r => r.ExistsByTitleAsync(It.IsAny<string>(), It.IsAny<int?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        _sut = new PositionUpdateDtoValidator(_positionRepository.Object);
    }

    private static ValidationContext<PositionUpdateDto> BuildContext(PositionUpdateDto dto, int? positionId = PositionId)
    {
        var context = new ValidationContext<PositionUpdateDto>(dto);
        if (positionId.HasValue)
        {
            context.RootContextData["PositionId"] = positionId.Value;
        }
        return context;
    }

    private static PositionUpdateDto ValidDto(string title = "Senior Analyst") => new()
    {
        Title = title,
        CareerLevel = CareerLevel.SeniorProfessional,
        IsCritical = false,
        IsActive = true
    };

    [Fact]
    public async Task ShouldNotHaveError_WhenTitleAlreadyExists_ExcludingItself()
    {
        // Arrange
        _positionRepository
            .Setup(r => r.ExistsByTitleAsync("Senior Analyst", PositionId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var dto = ValidDto();
        var context = BuildContext(dto);

        // Act
        var result = await _sut.TestValidateAsync(context);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.Title);

        _positionRepository.Verify(
            r => r.ExistsByTitleAsync("Senior Analyst", PositionId, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ShouldHaveError_WhenTitleBelongsToAnotherPosition()
    {
        // Arrange
        _positionRepository
            .Setup(r => r.ExistsByTitleAsync("Senior Analyst", PositionId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var dto = ValidDto();
        var context = BuildContext(dto);

        // Act
        var result = await _sut.TestValidateAsync(context);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Title)
            .WithErrorMessage("A position with this title already exists");
    }

    [Fact]
    public async Task ShouldHaveError_WhenCareerLevelIsInvalidEnumValue()
    {
        // Arrange
        var dto = ValidDto() with { CareerLevel = (CareerLevel)999 };
        var context = BuildContext(dto);

        // Act
        var result = await _sut.TestValidateAsync(context);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.CareerLevel)
            .WithErrorMessage("Invalid Career Level");
    }

    [Fact]
    public async Task ShouldThrow_WhenPositionIdIsMissingFromRootContextData()
    {
        // Arrange
        var dto = ValidDto();
        var context = BuildContext(dto, positionId: null);

        // Act
        Task Act() => _sut.ValidateAsync(context);

        // Assert
        await Assert.ThrowsAsync<InvalidOperationException>(Act);
    }
}