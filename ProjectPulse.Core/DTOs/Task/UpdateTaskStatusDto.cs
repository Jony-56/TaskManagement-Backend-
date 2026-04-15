using System.ComponentModel.DataAnnotations;

namespace ProjectPulse.Core.DTOs.Task;

public class UpdateTaskStatusDto
{
    [Required] public string Status { get; set; } = string.Empty; // Todo, InProgress, Review, Done
}