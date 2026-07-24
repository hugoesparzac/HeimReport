using HeimReport.Api.DTOs.Departments;
using HeimReport.Api.Entities;

namespace HeimReport.Api.Mappers;

public static class DepartmentMapper
{
    public static DepartmentResponseDto ToResponseDto(this Department department)
    {
        return new DepartmentResponseDto
        {
            Id = department.Id,
            Name = department.Name,
            IsActive = department.IsActive
        };
    }

    public static Department ToEntity(this DepartmentCreateDto dto)
    {
        return new Department
        {
            Name = dto.Name,
            IsActive = dto.IsActive
        };
    }

    public static void UpdateEntity(this DepartmentUpdateDto dto, Department department)
    {
        department.Name = dto.Name;
        department.IsActive = dto.IsActive;
    }
}