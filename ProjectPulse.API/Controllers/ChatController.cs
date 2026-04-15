using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProjectPulse.Core.Interfaces;

namespace ProjectPulse.API.Controllers;

[ApiController]
[Route("api/projects/{projectId}/chat")]
[Authorize]
public class ChatController : ControllerBase
{
    private readonly IChatService _chatService;
    public ChatController(IChatService chatService) => _chatService = chatService;

    private string UserId => User.FindFirstValue(ClaimTypes.NameIdentifier)!;

    // GET chat history
    [HttpGet]
    public async Task<IActionResult> GetMessages(int projectId, [FromQuery] int page = 1)
    {
        var messages = await _chatService.GetProjectMessagesAsync(projectId, UserId, page);
        return Ok(messages);
    }
}