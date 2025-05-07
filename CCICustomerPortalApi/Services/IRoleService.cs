using System.Collections.Generic;
using System.Threading.Tasks;
using CCICustomerPortalApi.Models;
using CCICustomerPortalApi.Models.DTOs;

namespace CCICustomerPortalApi.Services
{
    public interface IRoleService
    {
        Task<IEnumerable<RoleDto>> GetRolesAsync();
        Task<RoleDto?> GetRoleByIdAsync(string id);
        Task<RoleDto> CreateRoleAsync(CreateRoleDto dto);
        Task UpdateRoleAsync(string id, UpdateRoleDto dto);
        Task DeleteRoleAsync(string id);
        Task AssignRoleToUserAsync(string roleId, string userId);
        Task RemoveRoleFromUserAsync(string roleId, string userId);
        Task AssignRoleToWorkspaceAsync(string roleId, string workspaceId);
        Task RemoveRoleFromWorkspaceAsync(string roleId, string workspaceId);
        Task UpdateRolePermissionsAsync(string roleId, UpdateRolePermissionsDto dto);
    }
}
