namespace ProjectPulse.Core.DTOs.Task;

public class TaskDetailsDto
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string Status { get; set; } = string.Empty;
    public string Priority { get; set; } = string.Empty;
    public DateTime? DueDate { get; set; }
    public DateTime CreatedAt { get; set; }
    public int ProjectId { get; set; }
    public string? AssigneeId { get; set; }
    public string? AssigneeName { get; set; }
    public List<TaskCommentDto> Comments { get; set; } = new();
}

public class TaskCommentDto
{
    public int Id { get; set; }
    public string Content { get; set; } = string.Empty;
    public string AuthorName { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}