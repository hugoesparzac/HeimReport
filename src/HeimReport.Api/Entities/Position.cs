using HeimReport.Api.Enums;

namespace HeimReport.Api.Entities;

public class Position
{
    public int Id { get; set; }
    public required string Title { get; set; }
    public CareerLevel CareerLevel { get; set; }
    public bool IsCritical { get; set; }
    public required bool IsActive { get; set; }
}