using HeimReport.Api.Data;
using HeimReport.Api.Entities;
using Microsoft.EntityFrameworkCore;

namespace HeimReport.Api.Repositories.Employees;

public class EmployeeJobHistoryRepository(ApplicationDbContext context)
    : Repository<EmployeeJobHistory>(context), IEmployeeJobHistoryRepository
{
    public Task<EmployeeJobHistory?> GetOpenRecordAsync(int employeeId, CancellationToken cancellationToken = default)
    {
        return Context.Set<EmployeeJobHistory>()
            .Where(h => h.EmployeeId == employeeId && h.EndDate == null)
            .OrderByDescending(h => h.StartDate)
            .FirstOrDefaultAsync(cancellationToken);
    }
}