namespace ProjectPulse.Core.Entities;

public class ChatMessage : BaseEntity
{
    public string Content { get; set; } = string.Empty;

    public int ProjectId { get; set; }
    public Project Project { get; set; } = null!;

    public string SenderId { get; set; } = string.Empty;
    public AppUser Sender { get; set; } = null!;
}