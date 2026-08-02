using HeimReport.Api.DTOs.Positions;
using HeimReport.Api.Entities;
using HeimReport.Api.Enums;
using HeimReport.Api.Mappers;

namespace HeimReport.Api.UnitTests.Mappers;

public class PositionMapperTests
{
    [Fact]
    public void ToResponseDto_ShouldMapAllFields()
    {
        // Arrange
        var position = new Position
        {
            Id = 1,
            Title = "Analyst",
            CareerLevel = CareerLevel.Professional,
            IsCritical = false,
            IsActive = true
        };

        // Act
        var result = position.ToResponseDto();

        // Assert
        Assert.Equal(position.Id, result.Id);
        Assert.Equal(position.Title, result.Title);
        Assert.Equal(position.CareerLevel, result.CareerLevel);
        Assert.Equal(position.IsCritical, result.IsCritical);
        Assert.Equal(position.IsActive, result.IsActive);
    }

    [Fact]
    public void ToEntity_ShouldMapCreateDtoFields()
    {
        // Arrange
        var dto = new PositionCreateDto
        {
            Title = "Director",
            CareerLevel = CareerLevel.Executive,
            IsCritical = true,
            IsActive = true
        };

        // Act
        var entity = dto.ToEntity();

        // Assert
        Assert.Equal(dto.Title, entity.Title);
        Assert.Equal(dto.CareerLevel, entity.CareerLevel);
        Assert.Equal(dto.IsCritical, entity.IsCritical);
        Assert.Equal(dto.IsActive, entity.IsActive);
        Assert.Equal(0, entity.Id);
    }

    [Fact]
    public void UpdateEntity_ShouldMutateExistingInstance_WithoutCreatingNewOne()
    {
        // Arrange
        var entity = new Position
        {
            Id = 7,
            Title = "Old Title",
            CareerLevel = CareerLevel.Operational,
            IsCritical = false,
            IsActive = true
        };

        var dto = new PositionUpdateDto
        {
            Title = "New Title",
            CareerLevel = CareerLevel.SeniorProfessional,
            IsCritical = true,
            IsActive = false
        };

        // Act
        dto.UpdateEntity(entity);

        // Assert
        Assert.Equal(7, entity.Id);
        Assert.Equal("New Title", entity.Title);
        Assert.Equal(CareerLevel.SeniorProfessional, entity.CareerLevel);
        Assert.True(entity.IsCritical);
        Assert.False(entity.IsActive);
    }
}