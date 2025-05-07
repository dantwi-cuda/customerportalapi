using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CCICustomerPortalApi.Models;
using CCICustomerPortalApi.Models.DTOs;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using CCICustomerPortalApi.Data;

namespace CCICustomerPortalApi.Services
{
    public class RoleService : IRoleService
    {
        private readonly RoleManager<ApplicationRole> _roleManager;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ApplicationDbContext _db;

        public RoleService(RoleManager<ApplicationRole> roleManager, UserManager<ApplicationUser> userManager, ApplicationDbContext db)
        {
            _roleManager = roleManager;
            _userManager = userManager;
            _db = db;
        }

        public async Task<IEnumerable<RoleDto>> GetRolesAsync()
        {
            var roles = await _roleManager.Roles.Include(r => r.RolePermissions).ThenInclude(rp => rp.Permission).ToListAsync();
            return roles.Select(r => new RoleDto { Id = r.Id, Name = r.Name, Permissions = r.RolePermissions?.Select(rp => rp.Permission.Name).ToList() ?? new List<string>() });
        }

        public async Task<RoleDto?> GetRoleByIdAsync(string id)
        {
            var role = await _roleManager.Roles.Include(r => r.RolePermissions).ThenInclude(rp => rp.Permission).FirstOrDefaultAsync(r => r.Id == id);
            if (role == null) return null;
            var permissions = role.RolePermissions?.Select(rp => rp.Permission.Name).ToList() ?? new List<string>();
            return new RoleDto { Id = role.Id, Name = role.Name, Permissions = permissions };
        }

        public async Task<RoleDto> CreateRoleAsync(CreateRoleDto dto)
        {
            var role = new ApplicationRole { Name = dto.Name };
            var result = await _roleManager.CreateAsync(role);
            if (!result.Succeeded) throw new System.Exception(string.Join(", ", result.Errors.Select(e => e.Description)));
            // Assign permissions
            if (dto.Permissions != null && dto.Permissions.Any())
            {
                var permissions = await _db.Permissions.Where(p => dto.Permissions.Contains(p.Name)).ToListAsync();
                foreach (var perm in permissions)
                {
                    _db.RolePermissions.Add(new RolePermission { RoleId = role.Id, PermissionId = perm.Id });
                }
                await _db.SaveChangesAsync();
            }
            return await GetRoleByIdAsync(role.Id) ?? new RoleDto();
        }

        public async Task UpdateRoleAsync(string id, UpdateRoleDto dto)
        {
            var role = await _roleManager.FindByIdAsync(id);
            if (role == null) throw new System.Exception("Role not found");
            role.Name = dto.Name;
            await _roleManager.UpdateAsync(role);
            // Update permissions
            var current = _db.RolePermissions.Where(rp => rp.RoleId == id);
            _db.RolePermissions.RemoveRange(current);
            if (dto.Permissions != null && dto.Permissions.Any())
            {
                var permissions = await _db.Permissions.Where(p => dto.Permissions.Contains(p.Name)).ToListAsync();
                foreach (var perm in permissions)
                {
                    _db.RolePermissions.Add(new RolePermission { RoleId = id, PermissionId = perm.Id });
                }
            }
            await _db.SaveChangesAsync();
        }

        public async Task DeleteRoleAsync(string id)
        {
            var role = await _roleManager.FindByIdAsync(id);
            if (role == null) return;
            await _roleManager.DeleteAsync(role);
        }

        public async Task AssignRoleToUserAsync(string roleId, string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            var role = await _roleManager.FindByIdAsync(roleId);
            if (user == null || role == null) throw new System.Exception("User or Role not found");
            await _userManager.AddToRoleAsync(user, role.Name);
        }

        public async Task RemoveRoleFromUserAsync(string roleId, string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            var role = await _roleManager.FindByIdAsync(roleId);
            if (user == null || role == null) throw new System.Exception("User or Role not found");
            await _userManager.RemoveFromRoleAsync(user, role.Name);
        }

        public async Task AssignRoleToWorkspaceAsync(string roleId, string workspaceId)
        {
            // Implement if you have a join table for roles and workspaces
            // Example: _db.WorkspaceRoles.Add(new WorkspaceRole { RoleId = roleId, WorkspaceId = workspaceId });
            // await _db.SaveChangesAsync();
        }

        public async Task RemoveRoleFromWorkspaceAsync(string roleId, string workspaceId)
        {
            // Implement if you have a join table for roles and workspaces
            // Example: var wr = await _db.WorkspaceRoles.FirstOrDefaultAsync(x => x.RoleId == roleId && x.WorkspaceId == workspaceId);
            // if (wr != null) { _db.WorkspaceRoles.Remove(wr); await _db.SaveChangesAsync(); }
        }

        public async Task UpdateRolePermissionsAsync(string roleId, UpdateRolePermissionsDto dto)
        {
            var current = _db.RolePermissions.Where(rp => rp.RoleId == roleId);
            _db.RolePermissions.RemoveRange(current);
            if (dto.Permissions != null && dto.Permissions.Any())
            {
                var permissions = await _db.Permissions.Where(p => dto.Permissions.Contains(p.Name)).ToListAsync();
                foreach (var perm in permissions)
                {
                    _db.RolePermissions.Add(new RolePermission { RoleId = roleId, PermissionId = perm.Id });
                }
            }
            await _db.SaveChangesAsync();
        }
    }
}
