using System.ComponentModel.DataAnnotations;

namespace ProjectPulse.Core.DTOs.Project;

public class AddMemberDto
{
    [Required] public string UserId { get; set; } = string.Empty;
    public string Role { get; set; } = "Member";
}