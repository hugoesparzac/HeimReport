using HeimReport.Api.DTOs.Common;
using HeimReport.Api.Enums;

namespace HeimReport.Api.DTOs.Users;

public record UserQueryDto : PaginationQueryDto
{
    public string? SearchText { get; init; }
    public SystemRole? Role { get; init; }
    public bool? IsActive { get; init; }
}