namespace ProjectPulse.Core.Entities;

public class TaskItem : BaseEntity
{
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string Status { get; set; } = "Todo"; // Todo, InProgress, Review, Done
    public string Priority { get; set; } = "Medium"; // Low, Medium, High, Critical
    public DateTime? DueDate { get; set; }

    // Relations
    public int ProjectId { get; set; }
    public Project Project { get; set; } = null!;

    public string? AssigneeId { get; set; }
    public AppUser? Assignee { get; set; }

    public ICollection<TaskComment> Comments { get; set; } = new List<TaskComment>();
}