namespace ProjectPulse.Core.Entities;

public class Project : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string Status { get; set; } = "Active"; // Active, Archived, Completed
    public DateTime? Deadline { get; set; }

    // Owner
    public string OwnerId { get; set; } = string.Empty;
    public AppUser Owner { get; set; } = null!;

    // Navigation
    public ICollection<ProjectMember> Members { get; set; } = new List<ProjectMember>();
    public ICollection<TaskItem> Tasks { get; set; } = new List<TaskItem>();
    public ICollection<ChatMessage> ChatMessages { get; set; } = new List<ChatMessage>();
}