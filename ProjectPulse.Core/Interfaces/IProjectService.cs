using ProjectPulse.Core.DTOs.Project;

namespace ProjectPulse.Core.Interfaces;

public interface IProjectService
{
    Task<IEnumerable<ProjectDetailsDto>> GetAllProjectsAsync(string userId);
    Task<ProjectDetailsDto?> GetProjectByIdAsync(int projectId, string userId);
    Task<(bool Success, ProjectDetailsDto? Data, string Message)> CreateProjectAsync(string ownerId, CreateProjectDto dto);
    Task<(bool Success, string Message)> UpdateProjectAsync(int projectId, string userId, UpdateProjectDto dto);
    Task<(bool Success, string Message)> DeleteProjectAsync(int projectId, string userId);
    Task<(bool Success, string Message)> AddMemberAsync(int projectId, string requesterId, AddMemberDto dto);
    Task<(bool Success, string Message)> RemoveMemberAsync(int projectId, string requesterId, string userId);
}