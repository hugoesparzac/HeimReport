namespace HeimReport.Api.Entities;

public class MlModelVersion
{
    public int Id { get; set; }
    public required string Algorithm { get; set; }
    public required DateTime TrainedAt { get; set; }
    public string? Metrics { get; set; }
    public required bool IsActive { get; set; }
}