using HeimReport.Api.DTOs.Countries;
using HeimReport.Api.Repositories.Countries;
using HeimReport.Api.Validators.Countries;
using FluentValidation.TestHelper;
using Moq;

namespace HeimReport.Api.UnitTests.Validators.Countries;

public class CountryCreateDtoValidatorTests
{
    private readonly Mock<ICountryRepository> _countryRepository = new();
    private readonly CountryCreateDtoValidator _sut;

    public CountryCreateDtoValidatorTests()
    {
        _countryRepository
            .Setup(r => r.ExistsByNameAsync(It.IsAny<string>(), It.IsAny<int?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        _sut = new CountryCreateDtoValidator(_countryRepository.Object);
    }

    [Fact]
    public async Task ShouldHaveError_WhenNameIsEmpty()
    {
        // Arrange
        var dto = new CountryCreateDto { Name = "" };

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
        var dto = new CountryCreateDto { Name = new string('A', 61) };

        // Act
        var result = await _sut.TestValidateAsync(dto);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Name)
            .WithErrorMessage("Name cannot exceed 60 characters");
    }

    [Fact]
    public async Task ShouldHaveError_WhenNameAlreadyExists()
    {
        // Arrange
        _countryRepository
            .Setup(r => r.ExistsByNameAsync("Mexico", null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var dto = new CountryCreateDto { Name = "Mexico" };

        // Act
        var result = await _sut.TestValidateAsync(dto);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Name)
            .WithErrorMessage("A country with this name already exists");
    }

    [Fact]
    public async Task ShouldNotHaveAnyErrors_WhenDtoIsValid()
    {
        // Arrange
        var dto = new CountryCreateDto { Name = "Mexico" };

        // Act
        var result = await _sut.TestValidateAsync(dto);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }
}