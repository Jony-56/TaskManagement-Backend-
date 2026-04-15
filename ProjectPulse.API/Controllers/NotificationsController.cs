using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProjectPulse.Core.Interfaces;

namespace ProjectPulse.API.Controllers;

[ApiController]
[Route("api/notifications")]
[Authorize]
public class NotificationsController : ControllerBase
{
    private readonly INotificationService _notificationService;
    public NotificationsController(INotificationService notificationService) =>
        _notificationService = notificationService;

    private string UserId => User.FindFirstValue(ClaimTypes.NameIdentifier)!;

    [HttpGet]
    public async Task<IActionResult> GetAll() =>
        Ok(await _notificationService.GetUserNotificationsAsync(UserId));

    [HttpGet("unread-count")]
    public async Task<IActionResult> GetUnreadCount() =>
        Ok(new { count = await _notificationService.GetUnreadCountAsync(UserId) });

    [HttpPut("{id}/read")]
    public async Task<IActionResult> MarkAsRead(int id)
    {
        var (success, message) = await _notificationService.MarkAsReadAsync(id, UserId);
        return success ? Ok(new { message }) : BadRequest(new { message });
    }

    [HttpPut("read-all")]
    public async Task<IActionResult> MarkAllAsRead()
    {
        var (success, message) = await _notificationService.MarkAllAsReadAsync(UserId);
        return success ? Ok(new { message }) : BadRequest(new { message });
    }
}