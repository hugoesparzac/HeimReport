using FluentValidation;
using HeimReport.Api.DTOs.Common;
using HeimReport.Api.DTOs.Users;
using HeimReport.Api.Extensions;
using HeimReport.Api.Services.Users;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HeimReport.Api.Controllers;

[ApiController]
[Route("api/users")]
[Authorize]
public class UsersController(
    IUserService userService,
    IValidator<UserProvisionDto> provisionValidator,
    IValidator<UserUpdateDto> updateValidator,
    IValidator<ChangePasswordDto> changePasswordValidator) : ControllerBase
{
    [HttpGet]
    [Authorize(Roles = "Admin,HR")]
    public async Task<ActionResult<PagedResultDto<UserResponseDto>>> GetPaged(
        [FromQuery] UserQueryDto query, CancellationToken cancellationToken)
    {
        var result = await userService.GetPagedAsync(query, cancellationToken);
        return Ok(result);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<UserResponseDto>> GetById(int id, CancellationToken cancellationToken)
    {
        var requesterId = User.GetUserId();
        var requesterRole = User.GetSystemRole();

        var isOwnProfile = requesterId == id;
        var isPrivileged = requesterRole is Enums.SystemRole.HR or Enums.SystemRole.Admin;

        if (!isOwnProfile && !isPrivileged)
        {
            return Forbid();
        }

        var result = await userService.GetByIdAsync(id, cancellationToken);
        return Ok(result);
    }

    [HttpPost("provision")]
    [Authorize(Roles = "Admin,HR")]
    public async Task<ActionResult<UserResponseDto>> Provision(
        [FromBody] UserProvisionDto dto,
        CancellationToken cancellationToken)
    {
        var context = new ValidationContext<UserProvisionDto>(dto);
        context.RootContextData["RequesterRole"] = User.GetSystemRole();

        await provisionValidator.ValidateOrThrowAsync(context, cancellationToken);

        var result = await userService.ProvisionAsync(dto, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [HttpPut("{id:int}")]
    [Authorize(Roles = "Admin,HR")]
    public async Task<IActionResult> Update(
        int id,
        [FromBody] UserUpdateDto dto,
        CancellationToken cancellationToken)
    {
        var context = new ValidationContext<UserUpdateDto>(dto);
        context.RootContextData["RequesterRole"] = User.GetSystemRole();

        await updateValidator.ValidateOrThrowAsync(context, cancellationToken);

        await userService.UpdateAsync(id, dto, cancellationToken);
        return NoContent();
    }

    [HttpPost("change-password")]
    public async Task<IActionResult> ChangePassword(
        [FromBody] ChangePasswordDto dto,
        CancellationToken cancellationToken)
    {
        await changePasswordValidator.ValidateOrThrowAsync(dto, cancellationToken);

        var userId = User.GetUserId();
        await userService.ChangePasswordAsync(userId, dto, cancellationToken);
        return NoContent();
    }
}