using HeimReport.Api.Entities;

namespace HeimReport.Api.Repositories.Employees;

public interface IEmployeeJobHistoryRepository : IRepository<EmployeeJobHistory>
{
    Task<EmployeeJobHistory?> GetOpenRecordAsync(int employeeId, CancellationToken cancellationToken = default);
}