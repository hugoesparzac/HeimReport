using HeimReport.Api.DTOs.Users;
using HeimReport.Api.Services.Users;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HeimReport.Api.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController(IUserService userService) : ControllerBase
{
    [AllowAnonymous]
    [HttpPost("register")]
    public async Task<IActionResult> Register(
        [FromBody] UserRegistrationDto dto,
        CancellationToken cancellationToken)
    {
        var result = await userService.RegisterAsync(dto, cancellationToken);
        return CreatedAtAction(
            actionName: nameof(UsersController.GetById),
            controllerName: "Users",
            routeValues: new { id = result.Id },
            value: result);
    }

    [AllowAnonymous]
    [HttpGet("verify-email")]
    public async Task<IActionResult> VerifyEmail(
        [FromQuery] string token,
        CancellationToken cancellationToken)
    {
        await userService.VerifyEmailAsync(token, cancellationToken);
        return Ok(new { message = "Email verified successfully." });
    }

    [AllowAnonymous]
    [HttpPost("resend-verification")]
    public async Task<IActionResult> ResendVerification(
        [FromBody] ResendEmailVerificationDto dto,
        CancellationToken cancellationToken)
    {
        await userService.ResendVerificationAsync(dto, cancellationToken);
        return Ok(new { message = "If an account with that email exists, a verification link has been sent." });
    }

    [AllowAnonymous]
    [HttpPost("login")]
    public async Task<ActionResult<TokenResponseDto>> Login(
        [FromBody] UserLoginDto dto,
        CancellationToken cancellationToken)
    {
        var result = await userService.LoginAsync(dto, cancellationToken);
        return Ok(result);
    }

    [AllowAnonymous]
    [HttpPost("refresh-token")]
    public async Task<ActionResult<TokenResponseDto>> RefreshToken(
        [FromBody] RefreshTokenRequestDto dto,
        CancellationToken cancellationToken)
    {
        var result = await userService.RefreshAsync(dto.RefreshToken, cancellationToken);
        return Ok(result);
    }

    [Authorize]
    [HttpPost("logout")]
    public async Task<IActionResult> Logout(
        [FromBody] LogoutDto dto,
        CancellationToken cancellationToken)
    {
        await userService.LogoutAsync(dto.RefreshToken, cancellationToken);
        return NoContent();
    }
}
