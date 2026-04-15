using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProjectPulse.Core.DTOs.Task;
using ProjectPulse.Core.Interfaces;

namespace ProjectPulse.API.Controllers;

[ApiController]
[Route("api/projects/{projectId}/tasks")]
[Authorize]
public class TasksController : ControllerBase
{
    private readonly ITaskService _taskService;
    public TasksController(ITaskService taskService) => _taskService = taskService;

    private string UserId => User.FindFirstValue(ClaimTypes.NameIdentifier)!;

    [HttpGet]
    public async Task<IActionResult> GetAll(int projectId) =>
        Ok(await _taskService.GetTasksByProjectAsync(projectId, UserId));

    [HttpGet("{taskId}")]
    public async Task<IActionResult> GetById(int projectId, int taskId)
    {
        var task = await _taskService.GetTaskByIdAsync(taskId, UserId);
        return task is null ? NotFound() : Ok(task);
    }

    [HttpPost]
    public async Task<IActionResult> Create(int projectId, CreateTaskDto dto)
    {
        var (success, data, message) = await _taskService.CreateTaskAsync(projectId, UserId, dto);
        return success ? CreatedAtAction(nameof(GetById),
            new { projectId, taskId = data!.Id }, data)
            : BadRequest(new { message });
    }

    [HttpPut("{taskId}")]
    public async Task<IActionResult> Update(int projectId, int taskId, UpdateTaskDto dto)
    {
        var (success, message) = await _taskService.UpdateTaskAsync(taskId, UserId, dto);
        return success ? Ok(new { message }) : BadRequest(new { message });
    }

    [HttpPatch("{taskId}/status")]
    public async Task<IActionResult> UpdateStatus(int projectId, int taskId, UpdateTaskStatusDto dto)
    {
        var (success, message) = await _taskService.UpdateTaskStatusAsync(taskId, UserId, dto);
        return success ? Ok(new { message }) : BadRequest(new { message });
    }

    [HttpDelete("{taskId}")]
    public async Task<IActionResult> Delete(int projectId, int taskId)
    {
        var (success, message) = await _taskService.DeleteTaskAsync(taskId, UserId);
        return success ? Ok(new { message }) : BadRequest(new { message });
    }

    [HttpPost("{taskId}/comments")]
    public async Task<IActionResult> AddComment(int projectId, int taskId, AddCommentDto dto)
    {
        var (success, message) = await _taskService.AddCommentAsync(taskId, UserId, dto);
        return success ? Ok(new { message }) : BadRequest(new { message });
    }
}