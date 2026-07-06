namespace HeimReport.Api.Enums;

public enum EmployeeStatus
{
    Active = 1,
    VoluntaryResignation = 2,   // Left by choice (attrition/turnover)
    InvoluntaryTermination = 3, // Fired / laid off by the company
    ContractExpired = 4         // Contract reached its natural end date
}