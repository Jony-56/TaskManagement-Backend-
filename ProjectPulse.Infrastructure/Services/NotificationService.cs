using Microsoft.EntityFrameworkCore;
using ProjectPulse.Core.DTOs.Notification;
using ProjectPulse.Core.Entities;
using ProjectPulse.Core.Interfaces;
using ProjectPulse.Infrastructure.Data;

namespace ProjectPulse.Infrastructure.Services;

public class NotificationService : INotificationService
{
    private readonly AppDbContext _db;
    public NotificationService(AppDbContext db) => _db = db;

    private static NotificationDto MapToDto(Notification n) => new()
    {
        Id = n.Id,
        Title = n.Title,
        Message = n.Message,
        IsRead = n.IsRead,
        Type = n.Type,
        CreatedAt = n.CreatedAt
    };

    public async Task<IEnumerable<NotificationDto>> GetUserNotificationsAsync(string userId) =>
        await _db.Notifications
            .Where(n => n.UserId == userId)
            .OrderByDescending(n => n.CreatedAt)
            .Take(50)
            .Select(n => MapToDto(n))
            .ToListAsync();

    public async Task<int> GetUnreadCountAsync(string userId) =>
        await _db.Notifications.CountAsync(n => n.UserId == userId && !n.IsRead);

    public async Task<(bool Success, string Message)> MarkAsReadAsync(
        int notificationId, string userId)
    {
        var notification = await _db.Notifications
            .FirstOrDefaultAsync(n => n.Id == notificationId && n.UserId == userId);

        if (notification is null) return (false, "Notification not found.");

        notification.IsRead = true;
        await _db.SaveChangesAsync();
        return (true, "Notification marked as read.");
    }

    public async Task<(bool Success, string Message)> MarkAllAsReadAsync(string userId)
    {
        var notifications = await _db.Notifications
            .Where(n => n.UserId == userId && !n.IsRead)
            .ToListAsync();

        notifications.ForEach(n => n.IsRead = true);
        await _db.SaveChangesAsync();
        return (true, "All notifications marked as read.");
    }

    public async Task CreateNotificationAsync(
        string userId, string title, string message, string type = "Info")
    {
        _db.Notifications.Add(new Notification
        {
            UserId = userId,
            Title = title,
            Message = message,
            Type = type
        });
        await _db.SaveChangesAsync();
    }

    public async Task CreateProjectNotificationAsync(
        int projectId, string excludeUserId, string title, string message)
    {
        var memberIds = await _db.ProjectMembers
            .Where(m => m.ProjectId == projectId && m.UserId != excludeUserId)
            .Select(m => m.UserId)
            .ToListAsync();

        var project = await _db.Projects.FindAsync(projectId);
        if (project?.OwnerId != excludeUserId && !memberIds.Contains(project!.OwnerId))
            memberIds.Add(project.OwnerId);

        var notifications = memberIds.Select(uid => new Notification
        {
            UserId = uid,
            Title = title,
            Message = message,
            Type = "Info"
        });

        _db.Notifications.AddRange(notifications);
        await _db.SaveChangesAsync();
    }
}