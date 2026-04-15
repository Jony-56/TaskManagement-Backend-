namespace ProjectPulse.Core.Entities;

public class Notification : BaseEntity
{
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public bool IsRead { get; set; } = false;
    public string Type { get; set; } = "Info"; // Info, Warning, Success

    public string UserId { get; set; } = string.Empty;
    public AppUser User { get; set; } = null!;
}