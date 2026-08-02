using FluentValidation;
using FluentValidation.TestHelper;
using HeimReport.Api.DTOs.Countries;
using HeimReport.Api.Repositories.Countries;
using HeimReport.Api.Validators.Countries;
using Moq;

namespace HeimReport.Api.UnitTests.Validators.Countries;

public class CountryUpdateDtoValidatorTests
{
    private readonly Mock<ICountryRepository> _countryRepository = new();
    private readonly CountryUpdateDtoValidator _sut;

    private const int CountryId = 5;

    public CountryUpdateDtoValidatorTests()
    {
        _countryRepository
            .Setup(r => r.ExistsByNameAsync(It.IsAny<string>(), It.IsAny<int?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        _sut = new CountryUpdateDtoValidator(_countryRepository.Object);
    }

    private static ValidationContext<CountryUpdateDto> BuildContext(CountryUpdateDto dto, int? countryId = CountryId)
    {
        var context = new ValidationContext<CountryUpdateDto>(dto);
        if (countryId.HasValue)
        {
            context.RootContextData["CountryId"] = countryId.Value;
        }
        return context;
    }

    [Fact]
    public async Task ShouldNotHaveError_WhenNameAlreadyExists_ExcludingItself()
    {
        // Arrange
        // Confirma que se excluye el propio Id al validar unicidad en Update.
        _countryRepository
            .Setup(r => r.ExistsByNameAsync("Mexico", CountryId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var dto = new CountryUpdateDto { Name = "Mexico", IsActive = true };
        var context = BuildContext(dto);

        // Act
        var result = await _sut.TestValidateAsync(context);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.Name);

        _countryRepository.Verify(
            r => r.ExistsByNameAsync("Mexico", CountryId, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ShouldHaveError_WhenNameBelongsToAnotherCountry()
    {
        // Arrange
        _countryRepository
            .Setup(r => r.ExistsByNameAsync("Mexico", CountryId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var dto = new CountryUpdateDto { Name = "Mexico", IsActive = true };
        var context = BuildContext(dto);

        // Act
        var result = await _sut.TestValidateAsync(context);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Name)
            .WithErrorMessage("A country with this name already exists");
    }

    [Fact]
    public async Task ShouldThrow_WhenCountryIdIsMissingFromRootContextData()
    {
        // Arrange
        var dto = new CountryUpdateDto { Name = "Mexico", IsActive = true };
        var context = BuildContext(dto, countryId: null);

        // Act
        Task Act() => _sut.ValidateAsync(context);

        // Assert
        await Assert.ThrowsAsync<InvalidOperationException>(Act);
    }
}