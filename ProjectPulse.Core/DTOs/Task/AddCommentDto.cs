using System.ComponentModel.DataAnnotations;

namespace ProjectPulse.Core.DTOs.Task;

public class AddCommentDto
{
    [Required] public string Content { get; set; } = string.Empty;
}