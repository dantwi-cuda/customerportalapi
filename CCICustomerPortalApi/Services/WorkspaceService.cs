using AutoMapper;
using CCICustomerPortalApi.Data;
using CCICustomerPortalApi.Models;
using CCICustomerPortalApi.Models.DTOs;
using Microsoft.EntityFrameworkCore;

namespace CCICustomerPortalApi.Services;

public class WorkspaceService : IWorkspaceService
{
    private readonly ApplicationDbContext _context;
    private readonly IMapper _mapper;
    private readonly ITenantService _tenantService;

    public WorkspaceService(
        ApplicationDbContext context,
        IMapper mapper,
        ITenantService tenantService)
    {
        _context = context;
        _mapper = mapper;
        _tenantService = tenantService;
    }

    public async Task<WorkspaceDto> GetWorkspaceByIdAsync(int id)
    {
        var workspace = await _context.Workspaces
            .Include(w => w.Reports)
            .ThenInclude(r => r.Category)
            .FirstOrDefaultAsync(w => w.Id == id);

        if (workspace == null)
            throw new KeyNotFoundException($"Workspace with ID {id} not found.");

        return _mapper.Map<WorkspaceDto>(workspace);
    }

    public async Task<IEnumerable<WorkspaceDto>> GetWorkspacesAsync(bool includeReports = false)
    {
        IQueryable<Workspace> query = _context.Workspaces;

        if (includeReports)
        {
            query = query
                .Include(w => w.Reports)
                .ThenInclude(r => r.Category);
        }

        var workspaces = await query.ToListAsync();
        return _mapper.Map<IEnumerable<WorkspaceDto>>(workspaces);
    }

    public async Task<WorkspaceDto> CreateWorkspaceAsync(WorkspaceDto workspaceDto)
    {
        var workspace = _mapper.Map<Workspace>(workspaceDto);
        workspace.CustomerId = _tenantService.GetCurrentTenantId();

        _context.Workspaces.Add(workspace);
        await _context.SaveChangesAsync();

        return await GetWorkspaceByIdAsync(workspace.Id);
    }

    public async Task<WorkspaceDto> UpdateWorkspaceAsync(int id, WorkspaceDto workspaceDto)
    {
        var workspace = await _context.Workspaces.FindAsync(id);
        if (workspace == null)
            throw new KeyNotFoundException($"Workspace with ID {id} not found.");

        workspace.Name = workspaceDto.Name;
        workspace.SystemName = workspaceDto.SystemName;
        workspace.IsActive = workspaceDto.IsActive;

        await _context.SaveChangesAsync();
        return await GetWorkspaceByIdAsync(id);
    }

    public async Task<bool> DeleteWorkspaceAsync(int id)
    {
        var workspace = await _context.Workspaces.FindAsync(id);
        if (workspace == null)
            return false;

        _context.Workspaces.Remove(workspace);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> AssignUserToWorkspaceAsync(string userId, int workspaceId)
    {
        var userWorkspace = new UserWorkspace
        {
            UserId = userId,
            WorkspaceId = workspaceId,
            AssignedAt = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow
        };

        _context.UserWorkspaces.Add(userWorkspace);

        try
        {
            await _context.SaveChangesAsync();
            return true;
        }
        catch (DbUpdateException)
        {
            return false;
        }
    }

    public async Task<bool> RemoveUserFromWorkspaceAsync(string userId, int workspaceId)
    {
        var userWorkspace = await _context.UserWorkspaces
            .FirstOrDefaultAsync(uw => uw.UserId == userId && uw.WorkspaceId == workspaceId);

        if (userWorkspace == null)
            return false;

        _context.UserWorkspaces.Remove(userWorkspace);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> IsUserAssignedToWorkspaceAsync(string userId, int workspaceId)
    {
        return await _context.UserWorkspaces
            .AnyAsync(uw => uw.UserId == userId && uw.WorkspaceId == workspaceId);
    }
}