namespace HeimReport.Api.Entities;

public class EmployeeJobHistory
{
    public int Id { get; set; }
    public required int EmployeeId { get; set; }
    public Employee? Employee { get; set; }
    public required int DepartmentId { get; set; }
    public Department? Department { get; set; }
    public required int PositionId { get; set; }
    public Position? Position { get; set; }
    public int? ManagerId { get; set; }
    public Employee? Manager { get; set; }
    public decimal? Salary { get; set; }
    public required DateTime StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public string? ChangeReason { get; set; }
    public required DateTime CreatedAt { get; set; }
    public int? CreatedBy { get; set; }
}