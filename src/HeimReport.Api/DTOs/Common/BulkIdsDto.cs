namespace HeimReport.Api.DTOs.Common;

public record BulkIdsDto
{
    public required IReadOnlyList<int> Ids { get; init; }
}