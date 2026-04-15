using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProjectPulse.Core.DTOs.User;
using ProjectPulse.Core.Interfaces;

namespace ProjectPulse.API.Controllers;

[ApiController]
[Route("api/admin")]
[Authorize(Roles = "Admin")]
public class AdminController : ControllerBase
{
    private readonly IUserService _userService;
    public AdminController(IUserService userService) => _userService = userService;

    // GET all users
    [HttpGet("users")]
    public async Task<IActionResult> GetAllUsers()
    {
        var users = await _userService.GetAllUsersAsync();
        return Ok(users);
    }

    // GET user by id
    [HttpGet("users/{userId}")]
    public async Task<IActionResult> GetUserById(string userId)
    {
        var user = await _userService.GetUserByIdAsync(userId);
        return user is null ? NotFound() : Ok(user);
    }

    // POST assign role
    [HttpPost("users/assign-role")]
    public async Task<IActionResult> AssignRole(AssignRoleDto dto)
    {
        var (success, message) = await _userService.AssignRoleAsync(dto);
        return success ? Ok(new { message }) : BadRequest(new { message });
    }

    // DELETE remove role
    [HttpDelete("users/remove-role")]
    public async Task<IActionResult> RemoveRole(AssignRoleDto dto)
    {
        var (success, message) = await _userService.RemoveRoleAsync(dto);
        return success ? Ok(new { message }) : BadRequest(new { message });
    }

    // PUT deactivate user
    [HttpPut("users/{userId}/deactivate")]
    public async Task<IActionResult> Deactivate(string userId)
    {
        var (success, message) = await _userService.DeactivateUserAsync(userId);
        return success ? Ok(new { message }) : BadRequest(new { message });
    }

    // PUT activate user
    [HttpPut("users/{userId}/activate")]
    public async Task<IActionResult> Activate(string userId)
    {
        var (success, message) = await _userService.ActivateUserAsync(userId);
        return success ? Ok(new { message }) : BadRequest(new { message });
    }
}