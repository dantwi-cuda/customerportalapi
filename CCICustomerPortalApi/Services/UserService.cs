using AutoMapper;
using CCICustomerPortalApi.Data;
using CCICustomerPortalApi.Models;
using CCICustomerPortalApi.Models.DTOs;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace CCICustomerPortalApi.Services;

public class UserService : IUserService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ApplicationDbContext _context;
    private readonly IMapper _mapper;
    private readonly ITenantService _tenantService;

    public UserService(
        UserManager<ApplicationUser> userManager,
        ApplicationDbContext context,
        IMapper mapper,
        ITenantService tenantService)
    {
        _userManager = userManager;
        _context = context;
        _mapper = mapper;
        _tenantService = tenantService;
    }

    public async Task<UserDto> GetUserByIdAsync(string userId)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
            throw new KeyNotFoundException($"User with ID {userId} not found.");

        var dto = _mapper.Map<UserDto>(user);
        dto.Roles = (await _userManager.GetRolesAsync(user)).ToList();
        return dto;
    }

    public async Task<IEnumerable<UserDto>> GetUsersAsync(bool? isCustomerUser = null, bool? isCCIUser = null)
    {
        var query = _userManager.Users.AsQueryable();

        if (isCustomerUser.HasValue)
            query = query.Where(u => u.IsCustomerUser == isCustomerUser.Value);

        if (isCCIUser.HasValue)
            query = query.Where(u => u.IsCCIUser == isCCIUser.Value);

        // For customer users, filter by current tenant
        if (!isCCIUser ?? false)
        {
            var tenantId = _tenantService.GetCurrentTenantId();
            query = query.Where(u => u.CustomerUsers.Any(cu => cu.CustomerId == tenantId));
        }

        var users = await query.ToListAsync();
        var dtos = _mapper.Map<List<UserDto>>(users);

        // Load roles for each user
        for (int i = 0; i < users.Count; i++)
        {
            dtos[i].Roles = (await _userManager.GetRolesAsync(users[i])).ToList();
        }

        return dtos;
    }

    public async Task<UserDto> CreateUserAsync(UserDto userDto, string password, IEnumerable<string> roles)
    {
        var user = new ApplicationUser
        {
            UserName = userDto.Email,
            Email = userDto.Email,
            Name = userDto.Name,
            Status = userDto.Status,
            IsCustomerUser = userDto.IsCustomerUser,
            IsCCIUser = userDto.IsCCIUser,
            CreatedAt = DateTime.UtcNow
        };

        var result = await _userManager.CreateAsync(user, password);
        if (!result.Succeeded)
            throw new InvalidOperationException($"Failed to create user: {string.Join(", ", result.Errors.Select(e => e.Description))}");

        if (roles.Any())
        {
            result = await _userManager.AddToRolesAsync(user, roles);
            if (!result.Succeeded)
                throw new InvalidOperationException($"Failed to assign roles: {string.Join(", ", result.Errors.Select(e => e.Description))}");
        }

        return await GetUserByIdAsync(user.Id);
    }

    public async Task<UserDto> UpdateUserAsync(string userId, UserDto userDto)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
            throw new KeyNotFoundException($"User with ID {userId} not found.");

        user.Name = userDto.Name;
        user.Status = userDto.Status;
        user.IsCustomerUser = userDto.IsCustomerUser;
        user.IsCCIUser = userDto.IsCCIUser;

        var result = await _userManager.UpdateAsync(user);
        if (!result.Succeeded)
            throw new InvalidOperationException($"Failed to update user: {string.Join(", ", result.Errors.Select(e => e.Description))}");

        return await GetUserByIdAsync(userId);
    }

    public async Task<bool> DeleteUserAsync(string userId)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
            return false;

        var result = await _userManager.DeleteAsync(user);
        return result.Succeeded;
    }

    public async Task<bool> AssignToCustomerAsync(string userId, int customerId)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
            return false;

        var customerUser = new CustomerUser
        {
            UserId = userId,
            CustomerId = customerId,
            CreatedAt = DateTime.UtcNow
        };

        _context.CustomerUsers.Add(customerUser);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> RemoveFromCustomerAsync(string userId, int customerId)
    {
        var customerUser = await _context.CustomerUsers
            .FirstOrDefaultAsync(cu => cu.UserId == userId && cu.CustomerId == customerId);

        if (customerUser == null)
            return false;

        _context.CustomerUsers.Remove(customerUser);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> UpdateUserRolesAsync(string userId, IEnumerable<string> roles)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
            return false;

        var currentRoles = await _userManager.GetRolesAsync(user);
        await _userManager.RemoveFromRolesAsync(user, currentRoles);

        var result = await _userManager.AddToRolesAsync(user, roles);
        return result.Succeeded;
    }

    public async Task<bool> IsInCustomerAsync(string userId, int customerId)
    {
        return await _context.CustomerUsers
            .AnyAsync(cu => cu.UserId == userId && cu.CustomerId == customerId);
    }
}