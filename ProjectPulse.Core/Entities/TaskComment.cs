namespace ProjectPulse.Core.Entities;

public class TaskComment : BaseEntity
{
    public string Content { get; set; } = string.Empty;

    public int TaskItemId { get; set; }
    public TaskItem TaskItem { get; set; } = null!;

    public string AuthorId { get; set; } = string.Empty;
    public AppUser Author { get; set; } = null!;
}