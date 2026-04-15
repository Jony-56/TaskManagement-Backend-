using ProjectPulse.Core.DTOs.Notification;

namespace ProjectPulse.Core.Interfaces;

public interface INotificationService
{
    Task<IEnumerable<NotificationDto>> GetUserNotificationsAsync(string userId);
    Task<int> GetUnreadCountAsync(string userId);
    Task<(bool Success, string Message)> MarkAsReadAsync(int notificationId, string userId);
    Task<(bool Success, string Message)> MarkAllAsReadAsync(string userId);
    Task CreateNotificationAsync(string userId, string title, string message, string type = "Info");
    Task CreateProjectNotificationAsync(int projectId, string excludeUserId, string title, string message);
}