using HeimReport.Api.Entities;

namespace HeimReport.Api.Repositories.AuditLogs;

public interface IAuditLogRepository : IRepository<AuditLog>
{
    IQueryable<AuditLog> QueryWithDetails();
}