using System.ComponentModel.DataAnnotations;

namespace ProjectPulse.Core.DTOs.User;

public class AssignRoleDto
{
    [Required] public string UserId { get; set; } = string.Empty;
    [Required] public string Role { get; set; } = string.Empty; // Admin, ProjectManager, Member
}