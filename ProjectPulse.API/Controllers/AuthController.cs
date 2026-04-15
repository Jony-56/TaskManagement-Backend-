using Microsoft.AspNetCore.Mvc;
using ProjectPulse.Core.DTOs.Auth;
using ProjectPulse.Core.Interfaces;

namespace ProjectPulse.API.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    public AuthController(IAuthService authService) => _authService = authService;

    [HttpPost("register")]
    public async Task<IActionResult> Register(RegisterDto dto)
    {
        var (success, message) = await _authService.RegisterAsync(dto);
        return success ? Ok(new { message }) : BadRequest(new { message });
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginDto dto)
    {
        var (success, data, message) = await _authService.LoginAsync(dto);
        return success ? Ok(data) : Unauthorized(new { message });
    }

    [HttpGet("verify-email")]
    public async Task<IActionResult> VerifyEmail([FromQuery] string userId, [FromQuery] string token)
    {
        var (success, message) = await _authService.VerifyEmailAsync(userId, token);
        return success ? Ok(new { message }) : BadRequest(new { message });
    }

    [HttpPost("refresh-token")]
    public async Task<IActionResult> RefreshToken([FromBody] string refreshToken)
    {
        var (success, data, message) = await _authService.RefreshTokenAsync(refreshToken);
        return success ? Ok(data) : Unauthorized(new { message });
    }
}