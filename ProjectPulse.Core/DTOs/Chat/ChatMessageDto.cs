namespace ProjectPulse.Core.DTOs.Chat;

public class ChatMessageDto
{
    public int Id { get; set; }
    public string Content { get; set; } = string.Empty;
    public string SenderId { get; set; } = string.Empty;
    public string SenderName { get; set; } = string.Empty;
    public string? SenderAvatar { get; set; }
    public int ProjectId { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class SendMessageDto
{
    public string Content { get; set; } = string.Empty;
}