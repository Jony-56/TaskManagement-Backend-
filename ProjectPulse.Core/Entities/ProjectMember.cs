namespace ProjectPulse.Core.Entities;

public class ProjectMember
{
    public int Id { get; set; }
    public int ProjectId { get; set; }
    public Project Project { get; set; } = null!;

    public string UserId { get; set; } = string.Empty;
    public AppUser User { get; set; } = null!;

    public string Role { get; set; } = "Member"; // Manager, Member, Viewer
    public DateTime JoinedAt { get; set; } = DateTime.UtcNow;
}