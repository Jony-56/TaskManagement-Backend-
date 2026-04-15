using System.ComponentModel.DataAnnotations;

namespace ProjectPulse.Core.DTOs.Project;

public class CreateProjectDto
{
    [Required] public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTime? Deadline { get; set; }
}