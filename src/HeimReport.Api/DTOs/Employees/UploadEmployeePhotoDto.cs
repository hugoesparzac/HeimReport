using Microsoft.AspNetCore.Http;

namespace HeimReport.Api.DTOs.Employees;

public record UploadEmployeePhotoDto
{
    public required IFormFile Photo { get; init; }
}