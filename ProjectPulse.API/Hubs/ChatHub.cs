using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using ProjectPulse.Core.Interfaces;

namespace ProjectPulse.API.Hubs;

[Authorize]
public class ChatHub : Hub
{
    private readonly IChatService _chatService;
    private readonly INotificationService _notificationService;

    public ChatHub(IChatService chatService, INotificationService notificationService)
    {
        _chatService = chatService;
        _notificationService = notificationService;
    }

    // Join a project chat room
    public async Task JoinProjectRoom(int projectId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, $"project-{projectId}");
        await Clients.Caller.SendAsync("JoinedRoom", $"Joined project {projectId} chat.");
    }

    // Leave a project chat room
    public async Task LeaveProjectRoom(int projectId)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"project-{projectId}");
        await Clients.Caller.SendAsync("LeftRoom", $"Left project {projectId} chat.");
    }

    // Send message to project room
    public async Task SendMessage(int projectId, string content)
    {
        var userId = Context.User?.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId is null)
        {
            await Clients.Caller.SendAsync("Error", "Unauthorized.");
            return;
        }

        var (success, message, error) = await _chatService.SaveMessageAsync(projectId, userId, content);
        if (!success)
        {
            await Clients.Caller.SendAsync("Error", error);
            return;
        }

        // Broadcast to all in the project room
        await Clients.Group($"project-{projectId}")
            .SendAsync("ReceiveMessage", message);

        // Notify offline members
        await _notificationService.CreateProjectNotificationAsync(
            projectId, userId,
            "New Message",
            $"{message!.SenderName}: {content[..Math.Min(content.Length, 50)]}");
    }

    // Typing indicator
    public async Task Typing(int projectId)
    {
        var userName = Context.User?.FindFirstValue(ClaimTypes.Name);
        await Clients.OthersInGroup($"project-{projectId}")
            .SendAsync("UserTyping", userName);
    }

    public override async Task OnConnectedAsync()
    {
        var userId = Context.User?.FindFirstValue(ClaimTypes.NameIdentifier);
        await Clients.Others.SendAsync("UserOnline", userId);
        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var userId = Context.User?.FindFirstValue(ClaimTypes.NameIdentifier);
        await Clients.Others.SendAsync("UserOffline", userId);
        await base.OnDisconnectedAsync(exception);
    }
}