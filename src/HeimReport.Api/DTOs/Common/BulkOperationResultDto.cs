namespace HeimReport.Api.DTOs.Common;

public record BulkOperationResultDto
{
    public required IReadOnlyList<int> SucceededIds { get; init; }
    public required IReadOnlyList<BulkOperationErrorDto> Failed { get; init; }
}