using HeimReport.Api.DTOs.Departments;
using HeimReport.Api.Entities;

namespace HeimReport.Api.Mappers;

public static class DepartmentMapper
{
    public static DepartmentResponseDto ToResponseDto(this Department department)
    {
        return new DepartmentResponseDto(department.Id, department.Name);
    }

    public static Department ToEntity(this DepartmentCreateUpdateDto dto)
    {
        return new Department
        {
            Name = dto.Name,
        };
    }
}