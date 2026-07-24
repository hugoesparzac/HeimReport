namespace HeimReport.Api.DTOs.Common;

public record BulkOperationErrorDto
{
    public required int Id { get; init; }
    public required string Reason { get; init; }
}