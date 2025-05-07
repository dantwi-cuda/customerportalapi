using CCICustomerPortalApi.Models.DTOs;
using CCICustomerPortalApi.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CCICustomerPortalApi.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class WorkspaceController : ControllerBase
{
    private readonly IWorkspaceService _workspaceService;

    public WorkspaceController(IWorkspaceService workspaceService)
    {
        _workspaceService = workspaceService;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<WorkspaceDto>>> GetWorkspaces([FromQuery] bool includeReports = false)
    {
        var workspaces = await _workspaceService.GetWorkspacesAsync(includeReports);
        return Ok(workspaces);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<WorkspaceDto>> GetWorkspace(int id)
    {
        try
        {
            var workspace = await _workspaceService.GetWorkspaceByIdAsync(id);
            return Ok(workspace);
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
    }

    [Authorize(Roles = "Admin,CustomerAdmin")]
    [HttpPost]
    public async Task<ActionResult<WorkspaceDto>> CreateWorkspace([FromBody] WorkspaceDto workspaceDto)
    {
        var workspace = await _workspaceService.CreateWorkspaceAsync(workspaceDto);
        return CreatedAtAction(nameof(GetWorkspace), new { id = workspace.Id }, workspace);
    }

    [Authorize(Roles = "Admin,CustomerAdmin")]
    [HttpPut("{id}")]
    public async Task<ActionResult<WorkspaceDto>> UpdateWorkspace(int id, [FromBody] WorkspaceDto workspaceDto)
    {
        try
        {
            var workspace = await _workspaceService.UpdateWorkspaceAsync(id, workspaceDto);
            return Ok(workspace);
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
    }

    [Authorize(Roles = "Admin,CustomerAdmin")]
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteWorkspace(int id)
    {
        var result = await _workspaceService.DeleteWorkspaceAsync(id);
        if (!result)
            return NotFound();

        return NoContent();
    }

    [Authorize(Roles = "Admin,CustomerAdmin")]
    [HttpPost("{id}/users/{userId}")]
    public async Task<IActionResult> AssignUser(int id, string userId)
    {
        var result = await _workspaceService.AssignUserToWorkspaceAsync(userId, id);
        if (!result)
            return BadRequest();

        return NoContent();
    }

    [Authorize(Roles = "Admin,CustomerAdmin")]
    [HttpDelete("{id}/users/{userId}")]
    public async Task<IActionResult> RemoveUser(int id, string userId)
    {
        var result = await _workspaceService.RemoveUserFromWorkspaceAsync(userId, id);
        if (!result)
            return NotFound();

        return NoContent();
    }

    [HttpGet("{id}/users/{userId}")]
    public async Task<ActionResult<bool>> CheckUserAssignment(int id, string userId)
    {
        var result = await _workspaceService.IsUserAssignedToWorkspaceAsync(userId, id);
        return Ok(result);
    }
}