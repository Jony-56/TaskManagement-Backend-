using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using ProjectPulse.Core.DTOs.Task;
using ProjectPulse.Core.Entities;
using ProjectPulse.Core.Interfaces;
using ProjectPulse.Infrastructure.Data;

namespace ProjectPulse.Infrastructure.Services;

public class TaskService : ITaskService
{
    private readonly AppDbContext _db;
    private readonly IEmailService _emailService;
    private readonly UserManager<AppUser> _userManager;

    public TaskService(
        AppDbContext db,
        IEmailService emailService,
        UserManager<AppUser> userManager)
    {
        _db = db;
        _emailService = emailService;
        _userManager = userManager;
    }

    // ─── Helpers ──────────────────────────────────────────────

    private async Task<bool> HasProjectAccessAsync(int projectId, string userId) =>
        await _db.Projects.AnyAsync(p => p.Id == projectId &&
            (p.OwnerId == userId || p.Members.Any(m => m.UserId == userId)));

    private static TaskDetailsDto MapToDto(TaskItem task) => new()
    {
        Id = task.Id,
        Title = task.Title,
        Description = task.Description,
        Status = task.Status,
        Priority = task.Priority,
        DueDate = task.DueDate,
        CreatedAt = task.CreatedAt,
        ProjectId = task.ProjectId,
        AssigneeId = task.AssigneeId,
        AssigneeName = task.Assignee?.FullName,
        Comments = task.Comments?.Select(c => new TaskCommentDto
        {
            Id = c.Id,
            Content = c.Content,
            AuthorName = c.Author?.FullName ?? "",
            CreatedAt = c.CreatedAt
        }).OrderBy(c => c.CreatedAt).ToList() ?? new()
    };

    // ─── Queries ──────────────────────────────────────────────

    public async Task<IEnumerable<TaskDetailsDto>> GetTasksByProjectAsync(
        int projectId, string userId)
    {
        if (!await HasProjectAccessAsync(projectId, userId))
            return Enumerable.Empty<TaskDetailsDto>();

        var tasks = await _db.TaskItems
            .Include(t => t.Assignee)
            .Include(t => t.Comments).ThenInclude(c => c.Author)
            .Where(t => t.ProjectId == projectId)
            .OrderBy(t => t.CreatedAt)
            .ToListAsync();

        return tasks.Select(MapToDto);
    }

    public async Task<TaskDetailsDto?> GetTaskByIdAsync(int taskId, string userId)
    {
        var task = await _db.TaskItems
            .Include(t => t.Assignee)
            .Include(t => t.Comments).ThenInclude(c => c.Author)
            .FirstOrDefaultAsync(t => t.Id == taskId);

        if (task is null) return null;
        if (!await HasProjectAccessAsync(task.ProjectId, userId)) return null;

        return MapToDto(task);
    }

    // ─── Commands ─────────────────────────────────────────────

    public async Task<(bool Success, TaskDetailsDto? Data, string Message)> CreateTaskAsync(
        int projectId, string userId, CreateTaskDto dto)
    {
        if (!await HasProjectAccessAsync(projectId, userId))
            return (false, null, "You don't have access to this project.");

        var validPriorities = new[] { "Low", "Medium", "High", "Critical" };
        if (!validPriorities.Contains(dto.Priority))
            return (false, null, "Invalid priority. Use: Low, Medium, High, Critical");

        var task = new TaskItem
        {
            Title = dto.Title,
            Description = dto.Description,
            Priority = dto.Priority,
            DueDate = dto.DueDate,
            AssigneeId = dto.AssigneeId,
            ProjectId = projectId
        };

        _db.TaskItems.Add(task);
        await _db.SaveChangesAsync();

        // 📧 Notify assignee by email
        if (dto.AssigneeId is not null && dto.AssigneeId != userId)
        {
            var assignee = await _userManager.FindByIdAsync(dto.AssigneeId);
            var project = await _db.Projects.FindAsync(projectId);

            if (assignee?.Email is not null && project is not null)
            {
                try
                {
                    await _emailService.SendTaskAssignedEmailAsync(
                        assignee.Email,
                        assignee.FullName,
                        dto.Title,
                        project.Name,
                        dto.Priority,
                        dto.DueDate);
                }
                catch
                {
                    // Don't fail the request if email fails
                }
            }
        }

        var created = await _db.TaskItems
            .Include(t => t.Assignee)
            .Include(t => t.Comments).ThenInclude(c => c.Author)
            .FirstAsync(t => t.Id == task.Id);

        return (true, MapToDto(created), "Task created successfully.");
    }

    public async Task<(bool Success, string Message)> UpdateTaskAsync(
        int taskId, string userId, UpdateTaskDto dto)
    {
        var task = await _db.TaskItems.FindAsync(taskId);
        if (task is null) return (false, "Task not found.");

        if (!await HasProjectAccessAsync(task.ProjectId, userId))
            return (false, "You don't have access to this project.");

        var validStatuses = new[] { "Todo", "InProgress", "Review", "Done" };
        if (!validStatuses.Contains(dto.Status))
            return (false, "Invalid status. Use: Todo, InProgress, Review, Done");

        var validPriorities = new[] { "Low", "Medium", "High", "Critical" };
        if (!validPriorities.Contains(dto.Priority))
            return (false, "Invalid priority. Use: Low, Medium, High, Critical");

        // 📧 Notify new assignee if changed
        if (dto.AssigneeId is not null &&
            dto.AssigneeId != task.AssigneeId &&
            dto.AssigneeId != userId)
        {
            var assignee = await _userManager.FindByIdAsync(dto.AssigneeId);
            var project = await _db.Projects.FindAsync(task.ProjectId);

            if (assignee?.Email is not null && project is not null)
            {
                try
                {
                    await _emailService.SendTaskAssignedEmailAsync(
                        assignee.Email,
                        assignee.FullName,
                        dto.Title,
                        project.Name,
                        dto.Priority,
                        dto.DueDate);
                }
                catch { }
            }
        }

        task.Title = dto.Title;
        task.Description = dto.Description;
        task.Status = dto.Status;
        task.Priority = dto.Priority;
        task.DueDate = dto.DueDate;
        task.AssigneeId = dto.AssigneeId;
        task.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync();
        return (true, "Task updated successfully.");
    }

    public async Task<(bool Success, string Message)> UpdateTaskStatusAsync(
        int taskId, string userId, UpdateTaskStatusDto dto)
    {
        var task = await _db.TaskItems
            .Include(t => t.Assignee)
            .FirstOrDefaultAsync(t => t.Id == taskId);

        if (task is null) return (false, "Task not found.");

        if (!await HasProjectAccessAsync(task.ProjectId, userId))
            return (false, "You don't have access to this project.");

        var validStatuses = new[] { "Todo", "InProgress", "Review", "Done" };
        if (!validStatuses.Contains(dto.Status))
            return (false, "Invalid status. Use: Todo, InProgress, Review, Done");

        var oldStatus = task.Status;
        task.Status = dto.Status;
        task.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();

        // 📧 Notify assignee of status change
        if (task.AssigneeId is not null && task.AssigneeId != userId)
        {
            var project = await _db.Projects.FindAsync(task.ProjectId);
            var assignee = await _userManager.FindByIdAsync(task.AssigneeId);

            if (assignee?.Email is not null && project is not null)
            {
                try
                {
                    await _emailService.SendTaskStatusChangedEmailAsync(
                        assignee.Email,
                        assignee.FullName,
                        task.Title,
                        project.Name,
                        oldStatus,
                        dto.Status);
                }
                catch { }
            }
        }

        return (true, "Task status updated.");
    }

    public async Task<(bool Success, string Message)> DeleteTaskAsync(
        int taskId, string userId)
    {
        var task = await _db.TaskItems.FindAsync(taskId);
        if (task is null) return (false, "Task not found.");

        if (!await HasProjectAccessAsync(task.ProjectId, userId))
            return (false, "You don't have access to this project.");

        _db.TaskItems.Remove(task);
        await _db.SaveChangesAsync();
        return (true, "Task deleted successfully.");
    }

    public async Task<(bool Success, string Message)> AddCommentAsync(
        int taskId, string userId, AddCommentDto dto)
    {
        var task = await _db.TaskItems
            .Include(t => t.Assignee)
            .FirstOrDefaultAsync(t => t.Id == taskId);

        if (task is null) return (false, "Task not found.");

        if (!await HasProjectAccessAsync(task.ProjectId, userId))
            return (false, "You don't have access to this project.");

        if (string.IsNullOrWhiteSpace(dto.Content))
            return (false, "Comment cannot be empty.");

        _db.TaskComments.Add(new TaskComment
        {
            Content = dto.Content.Trim(),
            TaskItemId = taskId,
            AuthorId = userId
        });
        await _db.SaveChangesAsync();

        // 📧 Notify assignee of new comment
        if (task.AssigneeId is not null && task.AssigneeId != userId)
        {
            var commenter = await _userManager.FindByIdAsync(userId);
            var project = await _db.Projects.FindAsync(task.ProjectId);
            var assignee = await _userManager.FindByIdAsync(task.AssigneeId);

            if (assignee?.Email is not null && project is not null && commenter is not null)
            {
                try
                {
                    await _emailService.SendTaskCommentEmailAsync(
                        assignee.Email,
                        assignee.FullName,
                        task.Title,
                        project.Name,
                        commenter.FullName,
                        dto.Content);
                }
                catch { }
            }
        }

        return (true, "Comment added successfully.");
    }
}