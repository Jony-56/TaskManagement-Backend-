using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProjectPulse.Core.DTOs.Project;
using ProjectPulse.Core.Interfaces;

namespace ProjectPulse.API.Controllers;

[ApiController]
[Route("api/projects")]
[Authorize]
public class ProjectsController : ControllerBase
{
    private readonly IProjectService _projectService;
    public ProjectsController(IProjectService projectService) => _projectService = projectService;

    private string UserId => User.FindFirstValue(ClaimTypes.NameIdentifier)!;

    [HttpGet]
    public async Task<IActionResult> GetAll() =>
        Ok(await _projectService.GetAllProjectsAsync(UserId));

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var project = await _projectService.GetProjectByIdAsync(id, UserId);
        return project is null ? NotFound() : Ok(project);
    }

    [HttpPost]
    public async Task<IActionResult> Create(CreateProjectDto dto)
    {
        var (success, data, message) = await _projectService.CreateProjectAsync(UserId, dto);
        return success ? CreatedAtAction(nameof(GetById), new { id = data!.Id }, data)
                       : BadRequest(new { message });
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, UpdateProjectDto dto)
    {
        var (success, message) = await _projectService.UpdateProjectAsync(id, UserId, dto);
        return success ? Ok(new { message }) : BadRequest(new { message });
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var (success, message) = await _projectService.DeleteProjectAsync(id, UserId);
        return success ? Ok(new { message }) : BadRequest(new { message });
    }

    [HttpPost("{id}/members")]
    public async Task<IActionResult> AddMember(int id, AddMemberDto dto)
    {
        var (success, message) = await _projectService.AddMemberAsync(id, UserId, dto);
        return success ? Ok(new { message }) : BadRequest(new { message });
    }

    [HttpDelete("{id}/members/{userId}")]
    public async Task<IActionResult> RemoveMember(int id, string userId)
    {
        var (success, message) = await _projectService.RemoveMemberAsync(id, UserId, userId);
        return success ? Ok(new { message }) : BadRequest(new { message });
    }
}