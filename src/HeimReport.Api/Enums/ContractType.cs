namespace HeimReport.Api.Enums;

public enum ContractType
{
    FixedTerm = 1,   // Contract with a specific end date (e.g., 3 months, 1 year)
    Permanent = 2,   // Indefinite / permanent employment contract
    ProjectBased = 3 // Contract tied explicitly to a project's duration
}