using HeimReport.Api.DTOs.Departments;
using HeimReport.Api.Entities;
using HeimReport.Api.Mappers;

namespace HeimReport.Api.UnitTests.Mappers;

public class DepartmentMapperTests
{
    [Fact]
    public void ToResponseDto_ShouldMapAllFields()
    {
        // Arrange
        var department = new Department { Id = 1, Name = "Human Resources", IsActive = true };

        // Act
        var result = department.ToResponseDto();

        // Assert
        Assert.Equal(department.Id, result.Id);
        Assert.Equal(department.Name, result.Name);
        Assert.Equal(department.IsActive, result.IsActive);
    }

    [Fact]
    public void ToEntity_ShouldMapCreateDtoFields()
    {
        // Arrange
        var dto = new DepartmentCreateDto { Name = "Technology", IsActive = true };

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
        var entity = new Department { Id = 7, Name = "Old Name", IsActive = true };
        var dto = new DepartmentUpdateDto { Name = "New Name", IsActive = false };

        // Act
        dto.UpdateEntity(entity);

        // Assert
        Assert.Equal(7, entity.Id);
        Assert.Equal("New Name", entity.Name);
        Assert.False(entity.IsActive);
    }
}