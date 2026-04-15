using System.ComponentModel.DataAnnotations;

namespace ProjectPulse.Core.DTOs.Project;

public class UpdateProjectDto
{
    [Required] public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string Status { get; set; } = "Active";
    public DateTime? Deadline { get; set; }
}