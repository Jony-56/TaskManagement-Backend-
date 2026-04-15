using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using ProjectPulse.Core.Interfaces;

namespace ProjectPulse.API.Hubs;

[Authorize]
public class NotificationHub : Hub
{
    private readonly INotificationService _notificationService;

    public NotificationHub(INotificationService notificationService)
    {
        _notificationService = notificationService;
    }

    public override async Task OnConnectedAsync()
    {
        var userId = Context.User?.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId is not null)
        {
            // Join personal notification group
            await Groups.AddToGroupAsync(Context.ConnectionId, $"user-{userId}");

            // Send unread count on connect
            var count = await _notificationService.GetUnreadCountAsync(userId);
            await Clients.Caller.SendAsync("UnreadCount", count);
        }
        await base.OnConnectedAsync();
    }

    public async Task MarkAsRead(int notificationId)
    {
        var userId = Context.User?.FindFirstValue(ClaimTypes.NameIdentifier)!;
        await _notificationService.MarkAsReadAsync(notificationId, userId);
        var count = await _notificationService.GetUnreadCountAsync(userId);
        await Clients.Caller.SendAsync("UnreadCount", count);
    }

    public async Task MarkAllRead()
    {
        var userId = Context.User?.FindFirstValue(ClaimTypes.NameIdentifier)!;
        await _notificationService.MarkAllAsReadAsync(userId);
        await Clients.Caller.SendAsync("UnreadCount", 0);
    }
}