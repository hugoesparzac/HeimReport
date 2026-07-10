using HeimReport.Api.Enums;

namespace HeimReport.Api.Entities;

public class AttritionPrediction
{
    public int Id { get; set; }
    public required int EmployeeId { get; set; }
    public Employee? Employee { get; set; }
    public required int MlModelVersionId { get; set; }
    public MlModelVersion? MlModelVersion { get; set; }
    public required DateTime PredictionDate { get; set; }
    public required float RiskScore { get; set; }
    public required RiskLevel RiskLevel { get; set; }
}