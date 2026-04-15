using Microsoft.EntityFrameworkCore;
using ProjectPulse.Core.DTOs.Chat;
using ProjectPulse.Core.Entities;
using ProjectPulse.Core.Interfaces;
using ProjectPulse.Infrastructure.Data;

namespace ProjectPulse.Infrastructure.Services;

public class ChatService : IChatService
{
    private readonly AppDbContext _db;
    public ChatService(AppDbContext db) => _db = db;

    private async Task<bool> HasAccessAsync(int projectId, string userId) =>
        await _db.Projects.AnyAsync(p => p.Id == projectId &&
            (p.OwnerId == userId || p.Members.Any(m => m.UserId == userId)));

    private static ChatMessageDto MapToDto(ChatMessage msg) => new()
    {
        Id = msg.Id,
        Content = msg.Content,
        SenderId = msg.SenderId,
        SenderName = msg.Sender?.FullName ?? "",
        SenderAvatar = msg.Sender?.AvatarUrl,
        ProjectId = msg.ProjectId,
        CreatedAt = msg.CreatedAt
    };

    public async Task<IEnumerable<ChatMessageDto>> GetProjectMessagesAsync(
        int projectId, string userId, int page = 1)
    {
        if (!await HasAccessAsync(projectId, userId))
            return Enumerable.Empty<ChatMessageDto>();

        var pageSize = 50;
        return await _db.ChatMessages
            .Include(m => m.Sender)
            .Where(m => m.ProjectId == projectId)
            .OrderByDescending(m => m.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .OrderBy(m => m.CreatedAt)
            .Select(m => MapToDto(m))
            .ToListAsync();
    }

    public async Task<(bool Success, ChatMessageDto? Data, string Message)> SaveMessageAsync(
        int projectId, string senderId, string content)
    {
        if (!await HasAccessAsync(projectId, senderId))
            return (false, null, "You don't have access to this project.");

        if (string.IsNullOrWhiteSpace(content))
            return (false, null, "Message cannot be empty.");

        var message = new ChatMessage
        {
            Content = content.Trim(),
            ProjectId = projectId,
            SenderId = senderId
        };

        _db.ChatMessages.Add(message);
        await _db.SaveChangesAsync();

        var saved = await _db.ChatMessages
            .Include(m => m.Sender)
            .FirstAsync(m => m.Id == message.Id);

        return (true, MapToDto(saved), "Message sent.");
    }
}