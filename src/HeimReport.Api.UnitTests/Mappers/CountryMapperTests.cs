using HeimReport.Api.DTOs.Countries;
using HeimReport.Api.Entities;
using HeimReport.Api.Mappers;

namespace HeimReport.Api.UnitTests.Mappers;

public class CountryMapperTests
{
    [Fact]
    public void ToResponseDto_ShouldMapAllFields()
    {
        // Arrange
        var country = new Country { Id = 1, Name = "Mexico", IsActive = true };

        // Act
        var result = country.ToResponseDto();

        // Assert
        Assert.Equal(country.Id, result.Id);
        Assert.Equal(country.Name, result.Name);
        Assert.Equal(country.IsActive, result.IsActive);
    }

    [Fact]
    public void ToEntity_ShouldMapCreateDtoFields()
    {
        // Arrange
        var dto = new CountryCreateDto { Name = "Colombia", IsActive = true };

        // Act
        var entity = dto.ToEntity();

        // Assert
        Assert.Equal(dto.Name, entity.Name);
        Assert.Equal(dto.IsActive, entity.IsActive);
        Assert.Equal(0, entity.Id);
    }

    [Fact]
    public void UpdateEntity_ShouldMutateExistingInstance_WithoutCreatingNewOne()
    {
        // Arrange
        var entity = new Country { Id = 7, Name = "Old Name", IsActive = true };
        var dto = new CountryUpdateDto { Name = "New Name", IsActive = false };

        // Act
        dto.UpdateEntity(entity);

        // Assert
        Assert.Equal(7, entity.Id);
        Assert.Equal("New Name", entity.Name);
        Assert.False(entity.IsActive);
    }
}