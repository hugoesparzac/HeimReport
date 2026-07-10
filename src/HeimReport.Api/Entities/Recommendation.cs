using HeimReport.Api.Enums;

namespace HeimReport.Api.Entities;

public class Recommendation
{
    public int Id { get; set; }
    public required int AttritionPredictionId { get; set; }
    public AttritionPrediction? AttritionPrediction { get; set; }
    public required string SuggestionText { get; set; }
    public string? Category { get; set; }
    public required RecommendationStatus Status { get; set; }
    public required DateTime CreatedAt { get; set; }
}