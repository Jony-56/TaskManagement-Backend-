using System.ComponentModel.DataAnnotations;

namespace ProjectPulse.Core.DTOs.Task;

public class UpdateTaskDto
{
    [Required] public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string Status { get; set; } = "Todo";
    public string Priority { get; set; } = "Medium";
    public DateTime? DueDate { get; set; }
    public string? AssigneeId { get; set; }
}