using ProjectPulse.Core.DTOs.Chat;

namespace ProjectPulse.Core.Interfaces;

public interface IChatService
{
    Task<IEnumerable<ChatMessageDto>> GetProjectMessagesAsync(int projectId, string userId, int page = 1);
    Task<(bool Success, ChatMessageDto? Data, string Message)> SaveMessageAsync(
        int projectId, string senderId, string content);
}