namespace HeimReport.Api.DTOs.Common;

public record PaginationQueryDto
{
    private const int MaxPageSize = 100;
    public int PageNumber { get; init; } = 1;
    public int _pageSize = 20;
    public int PageSize
    {
        get => _pageSize;
        init => _pageSize = value > MaxPageSize ? MaxPageSize : value;
    }
}
