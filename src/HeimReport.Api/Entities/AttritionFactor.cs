namespace HeimReport.Api.Entities;

public class AttritionFactor
{
    public int Id { get; set; }
    public required int AttritionPredictionId { get; set; }
    public AttritionPrediction? AttritionPrediction { get; set; }
    public required string Name { get; set; }
    public required float ContributionScore { get; set; }
}