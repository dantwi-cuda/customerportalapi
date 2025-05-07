using CCICustomerPortalApi.Models.DTOs;

namespace CCICustomerPortalApi.Services;

public interface IWorkspaceService
{
    Task<WorkspaceDto> GetWorkspaceByIdAsync(int id);
    Task<IEnumerable<WorkspaceDto>> GetWorkspacesAsync(bool includeReports = false);
    Task<WorkspaceDto> CreateWorkspaceAsync(WorkspaceDto workspace);
    Task<WorkspaceDto> UpdateWorkspaceAsync(int id, WorkspaceDto workspace);
    Task<bool> DeleteWorkspaceAsync(int id);
    Task<bool> AssignUserToWorkspaceAsync(string userId, int workspaceId);
    Task<bool> RemoveUserFromWorkspaceAsync(string userId, int workspaceId);
    Task<bool> IsUserAssignedToWorkspaceAsync(string userId, int workspaceId);
}