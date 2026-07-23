using HeimReport.Api.DTOs.Users;
using HeimReport.Api.Services.Users;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace HeimReport.Api.Controllers;

[ApiController]
[Route("api/users")]
[Authorize]
public class UsersController(IUserService userService) : ControllerBase
{
    [HttpGet("{id:int}")]
    public async Task<ActionResult<UserResponseDto>> GetById(int id, CancellationToken cancellationToken)
    {
        var requesterId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? User.FindFirstValue("sub")!);
        var requesterRole = User.FindFirstValue(ClaimTypes.Role);

        var isOwnProfile = requesterId == id;
        var isPrivileged = requesterRole is "HR" or "Admin";

        if (!isOwnProfile && !isPrivileged)
        {
            return Forbid();
        }

        var result = await userService.GetByIdAsync(id, cancellationToken);
        return Ok(result);
    }
}