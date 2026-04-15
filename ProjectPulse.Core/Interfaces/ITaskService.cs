using ProjectPulse.Core.DTOs.Task;

namespace ProjectPulse.Core.Interfaces;

public interface ITaskService
{
    Task<IEnumerable<TaskDetailsDto>> GetTasksByProjectAsync(int projectId, string userId);
    Task<TaskDetailsDto?> GetTaskByIdAsync(int taskId, string userId);
    Task<(bool Success, TaskDetailsDto? Data, string Message)> CreateTaskAsync(int projectId, string userId, CreateTaskDto dto);
    Task<(bool Success, string Message)> UpdateTaskAsync(int taskId, string userId, UpdateTaskDto dto);
    Task<(bool Success, string Message)> UpdateTaskStatusAsync(int taskId, string userId, UpdateTaskStatusDto dto);
    Task<(bool Success, string Message)> DeleteTaskAsync(int taskId, string userId);
    Task<(bool Success, string Message)> AddCommentAsync(int taskId, string userId, AddCommentDto dto);
}