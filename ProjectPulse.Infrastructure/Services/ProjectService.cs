using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using ProjectPulse.Core.DTOs.Project;
using ProjectPulse.Core.Entities;
using ProjectPulse.Core.Interfaces;
using ProjectPulse.Infrastructure.Data;

namespace ProjectPulse.Infrastructure.Services;

public class ProjectService : IProjectService
{
    private readonly AppDbContext _db;
    private readonly IEmailService _emailService;
    private readonly UserManager<AppUser> _userManager;

    public ProjectService(
        AppDbContext db,
        IEmailService emailService,
        UserManager<AppUser> userManager)
    {
        _db = db;
        _emailService = emailService;
        _userManager = userManager;
    }

    // ─── Helpers ──────────────────────────────────────────────

    private async Task<bool> IsMemberAsync(int projectId, string userId) =>
        await _db.ProjectMembers.AnyAsync(pm =>
            pm.ProjectId == projectId && pm.UserId == userId);

    private async Task<bool> IsOwnerOrManagerAsync(int projectId, string userId)
    {
        var project = await _db.Projects.FindAsync(projectId);
        if (project?.OwnerId == userId) return true;
        return await _db.ProjectMembers.AnyAsync(pm =>
            pm.ProjectId == projectId &&
            pm.UserId == userId &&
            pm.Role == "Manager");
    }

    private static ProjectDetailsDto MapToDto(Project project) => new()
    {
        Id = project.Id,
        Name = project.Name,
        Description = project.Description,
        Status = project.Status,
        Deadline = project.Deadline,
        CreatedAt = project.CreatedAt,
        OwnerName = project.Owner?.FullName ?? "",
        MemberCount = project.Members?.Count ?? 0,
        TaskCount = project.Tasks?.Count ?? 0,
        Members = project.Members?.Select(m => new ProjectMemberDto
        {
            UserId = m.UserId,
            FullName = m.User?.FullName ?? "",
            Email = m.User?.Email ?? "",
            Role = m.Role
        }).ToList() ?? new()
    };

    // ─── Queries ──────────────────────────────────────────────

    public async Task<IEnumerable<ProjectDetailsDto>> GetAllProjectsAsync(string userId)
    {
        var projects = await _db.Projects
            .Include(p => p.Owner)
            .Include(p => p.Members).ThenInclude(m => m.User)
            .Include(p => p.Tasks)
            .Where(p => p.OwnerId == userId ||
                        p.Members.Any(m => m.UserId == userId))
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync();

        return projects.Select(MapToDto);
    }

    public async Task<ProjectDetailsDto?> GetProjectByIdAsync(int projectId, string userId)
    {
        var project = await _db.Projects
            .Include(p => p.Owner)
            .Include(p => p.Members).ThenInclude(m => m.User)
            .Include(p => p.Tasks)
            .FirstOrDefaultAsync(p => p.Id == projectId);

        if (project is null) return null;
        if (project.OwnerId != userId && !await IsMemberAsync(projectId, userId))
            return null;

        return MapToDto(project);
    }

    // ─── Commands ─────────────────────────────────────────────

    public async Task<(bool Success, ProjectDetailsDto? Data, string Message)> CreateProjectAsync(
        string ownerId, CreateProjectDto dto)
    {
        var project = new Project
        {
            Name = dto.Name,
            Description = dto.Description,
            Deadline = dto.Deadline,
            OwnerId = ownerId
        };

        _db.Projects.Add(project);
        await _db.SaveChangesAsync();

        // Add owner as Manager member automatically
        _db.ProjectMembers.Add(new ProjectMember
        {
            ProjectId = project.Id,
            UserId = ownerId,
            Role = "Manager"
        });
        await _db.SaveChangesAsync();

        var created = await _db.Projects
            .Include(p => p.Owner)
            .Include(p => p.Members).ThenInclude(m => m.User)
            .Include(p => p.Tasks)
            .FirstAsync(p => p.Id == project.Id);

        return (true, MapToDto(created), "Project created successfully.");
    }

    public async Task<(bool Success, string Message)> UpdateProjectAsync(
        int projectId, string userId, UpdateProjectDto dto)
    {
        var project = await _db.Projects.FindAsync(projectId);
        if (project is null) return (false, "Project not found.");

        if (!await IsOwnerOrManagerAsync(projectId, userId))
            return (false, "You don't have permission to update this project.");

        project.Name = dto.Name;
        project.Description = dto.Description;
        project.Status = dto.Status;
        project.Deadline = dto.Deadline;
        project.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync();
        return (true, "Project updated successfully.");
    }

    public async Task<(bool Success, string Message)> DeleteProjectAsync(
        int projectId, string userId)
    {
        var project = await _db.Projects.FindAsync(projectId);
        if (project is null) return (false, "Project not found.");

        if (project.OwnerId != userId)
            return (false, "Only the project owner can delete it.");

        _db.Projects.Remove(project);
        await _db.SaveChangesAsync();
        return (true, "Project deleted successfully.");
    }

    public async Task<(bool Success, string Message)> AddMemberAsync(
        int projectId, string requesterId, AddMemberDto dto)
    {
        if (!await IsOwnerOrManagerAsync(projectId, requesterId))
            return (false, "You don't have permission to add members.");

        if (await IsMemberAsync(projectId, dto.UserId))
            return (false, "User is already a member of this project.");

        var validRoles = new[] { "Manager", "Member", "Viewer" };
        if (!validRoles.Contains(dto.Role))
            return (false, "Invalid role. Use: Manager, Member, Viewer");

        _db.ProjectMembers.Add(new ProjectMember
        {
            ProjectId = projectId,
            UserId = dto.UserId,
            Role = dto.Role
        });
        await _db.SaveChangesAsync();

        // 📧 Send invite email
        var project = await _db.Projects.FindAsync(projectId);
        var invitedUser = await _userManager.FindByIdAsync(dto.UserId);
        var inviter = await _userManager.FindByIdAsync(requesterId);

        if (invitedUser?.Email is not null && project is not null && inviter is not null)
        {
            try
            {
                await _emailService.SendProjectInviteEmailAsync(
                    invitedUser.Email,
                    invitedUser.FullName,
                    project.Name,
                    inviter.FullName);
            }
            catch
            {
                // Don't fail the request if email fails
            }
        }

        return (true, "Member added successfully.");
    }

    public async Task<(bool Success, string Message)> RemoveMemberAsync(
        int projectId, string requesterId, string userId)
    {
        if (!await IsOwnerOrManagerAsync(projectId, requesterId))
            return (false, "You don't have permission to remove members.");

        var project = await _db.Projects.FindAsync(projectId);
        if (project?.OwnerId == userId)
            return (false, "Cannot remove the project owner.");

        var member = await _db.ProjectMembers
            .FirstOrDefaultAsync(m => m.ProjectId == projectId && m.UserId == userId);

        if (member is null) return (false, "Member not found in this project.");

        _db.ProjectMembers.Remove(member);
        await _db.SaveChangesAsync();

        // 📧 Send removed email
        var removedUser = await _userManager.FindByIdAsync(userId);

        if (removedUser?.Email is not null && project is not null)
        {
            try
            {
                await _emailService.SendProjectRemovedEmailAsync(
                    removedUser.Email,
                    removedUser.FullName,
                    project.Name);
            }
            catch
            {
                // Don't fail the request if email fails
            }
        }

        return (true, "Member removed successfully.");
    }
}