using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProjectPulse.Core.DTOs.User;
using ProjectPulse.Core.Interfaces;
using System.Security.Claims;

[ApiController]
[Route("api/users")]
[Authorize]
public class UsersController : ControllerBase
{
    private readonly IUserService _userService;
    public UsersController(IUserService userService) => _userService = userService;

    // ✅ NEW — Any logged-in user can get all users (for adding members)
    [HttpGet]
    public async Task<IActionResult> GetAllUsers()
    {
        var users = await _userService.GetAllUsersAsync();
        return Ok(users);
    }

    // GET current logged-in user profile
    [HttpGet("me")]
    public async Task<IActionResult> GetMe()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var user = await _userService.GetCurrentUserAsync(userId);
        return user is null ? NotFound() : Ok(user);
    }

    // PUT update own profile
    [HttpPut("me")]
    public async Task<IActionResult> UpdateMe(UpdateUserDto dto)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var (success, message) = await _userService.UpdateUserAsync(userId, dto);
        return success ? Ok(new { message }) : BadRequest(new { message });
    }

    // PUT change own password
    [HttpPut("me/change-password")]
    public async Task<IActionResult> ChangePassword(ChangePasswordDto dto)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var (success, message) = await _userService.ChangePasswordAsync(userId, dto);
        return success ? Ok(new { message }) : BadRequest(new { message });
    }
}