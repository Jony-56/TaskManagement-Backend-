using System.ComponentModel.DataAnnotations;

namespace ProjectPulse.Core.DTOs.User;

public class UpdateUserDto
{
    [Required] public string FullName { get; set; } = string.Empty;
    public string? AvatarUrl { get; set; }
}