using HeimReport.Api.Data;
using HeimReport.Api.Entities;
using Microsoft.EntityFrameworkCore;

namespace HeimReport.Api.Repositories.AuditLogs;

public class AuditLogRepository(ApplicationDbContext context) : Repository<AuditLog>(context), IAuditLogRepository
{
    public IQueryable<AuditLog> QueryWithDetails()
    {
        return Context.Set<AuditLog>()
            .Include(a => a.User)
            .ThenInclude(u => u!.Employee);
    }
}