namespace HeimReport.Api.Entities;

public class Department
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public required bool IsActive { get; set; }
}