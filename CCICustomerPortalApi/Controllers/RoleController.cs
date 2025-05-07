using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;
using CCICustomerPortalApi.Models.DTOs;
using CCICustomerPortalApi.Services;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using CCICustomerPortalApi.Data;
using CCICustomerPortalApi.Models;

namespace CCICustomerPortalApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Policy = "RequireCustomerAdmin")]
    public class RoleController : ControllerBase
    {
        private readonly IRoleService _roleService;

        public RoleController(IRoleService roleService)
        {
            _roleService = roleService;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<RoleDto>>> GetRoles()
            => Ok(await _roleService.GetRolesAsync());

        [HttpGet("{id}")]
        public async Task<ActionResult<RoleDto>> GetRole(string id)
            => Ok(await _roleService.GetRoleByIdAsync(id));

        [HttpPost]
        public async Task<ActionResult<RoleDto>> CreateRole([FromBody] CreateRoleDto dto)
            => Ok(await _roleService.CreateRoleAsync(dto));

        [HttpPut("{id}")]
        public async Task<ActionResult> UpdateRole(string id, [FromBody] UpdateRoleDto dto)
        {
            await _roleService.UpdateRoleAsync(id, dto);
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteRole(string id)
        {
            await _roleService.DeleteRoleAsync(id);
            return NoContent();
        }

        [HttpPost("{id}/assign-user")]
        public async Task<ActionResult> AssignRoleToUser(string id, [FromBody] AssignRoleDto dto)
        {
            await _roleService.AssignRoleToUserAsync(id, dto.UserId);
            return NoContent();
        }

        [HttpPost("{id}/remove-user")]
        public async Task<ActionResult> RemoveRoleFromUser(string id, [FromBody] AssignRoleDto dto)
        {
            await _roleService.RemoveRoleFromUserAsync(id, dto.UserId);
            return NoContent();
        }

        [HttpPost("{id}/assign-workspace")]
        public async Task<ActionResult> AssignRoleToWorkspace(string id, [FromBody] AssignRoleDto dto)
        {
            await _roleService.AssignRoleToWorkspaceAsync(id, dto.WorkspaceId);
            return NoContent();
        }

        [HttpPost("{id}/remove-workspace")]
        public async Task<ActionResult> RemoveRoleFromWorkspace(string id, [FromBody] AssignRoleDto dto)
        {
            await _roleService.RemoveRoleFromWorkspaceAsync(id, dto.WorkspaceId);
            return NoContent();
        }

        [HttpPut("{id}/permissions")]
        public async Task<ActionResult> UpdateRolePermissions(string id, [FromBody] UpdateRolePermissionsDto dto)
        {
            await _roleService.UpdateRolePermissionsAsync(id, dto);
            return NoContent();
        }

        [HttpGet("permissions")]
        public async Task<ActionResult<IEnumerable<string>>> GetAllPermissions([FromServices] ApplicationDbContext db)
        {
            var permissions = await db.Permissions.Select(p => p.Name).ToListAsync();
            return Ok(permissions);
        }
    }
}
