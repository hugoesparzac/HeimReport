using HeimReport.Api.DTOs.AuditLogs;
using HeimReport.Api.Entities;

namespace HeimReport.Api.Mappers;

public static class AuditLogMapper
{
    public static AuditLogResponseDto ToResponseDto(this AuditLog log)
    {
        return new AuditLogResponseDto
        {
            Id = log.Id,
            UserId = log.UserId,
            UserFullName = log.User?.Employee is not null
                ? $"{log.User.Employee.FirstName} {log.User.Employee.LastName}"
                : null,
            Action = log.Action,
            EntityName = log.EntityName,
            EntityId = log.EntityId,
            OldValues = log.OldValues,
            NewValues = log.NewValues,
            IpAddress = log.IpAddress,
            Timestamp = log.Timestamp
        };
    }
}