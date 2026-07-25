using HeimReport.Api.Enums;

namespace HeimReport.Api.Security;

public static class RoleAssignmentPolicy
{
    /// <summary>
    /// Admin can assign any role. HR can only assign the Employee or HR role, never Admin.
    /// Any other requesting role cannot assign any roles.
    /// </summary>
    public static bool CanAssign(SystemRole requesterRole, SystemRole targetRole) =>
        requesterRole switch
        {
            SystemRole.Admin => true,
            SystemRole.HR => targetRole is SystemRole.Employee or SystemRole.HR,
            _ => false
        };
}