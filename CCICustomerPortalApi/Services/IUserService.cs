using CCICustomerPortalApi.Models.DTOs;

namespace CCICustomerPortalApi.Services;

public interface IUserService
{
    Task<UserDto> GetUserByIdAsync(string userId);
    Task<IEnumerable<UserDto>> GetUsersAsync(bool? isCustomerUser = null, bool? isCCIUser = null);
    Task<UserDto> CreateUserAsync(UserDto userDto, string password, IEnumerable<string> roles);
    Task<UserDto> UpdateUserAsync(string userId, UserDto userDto);
    Task<bool> DeleteUserAsync(string userId);
    Task<bool> AssignToCustomerAsync(string userId, int customerId);
    Task<bool> RemoveFromCustomerAsync(string userId, int customerId);
    Task<bool> UpdateUserRolesAsync(string userId, IEnumerable<string> roles);
    Task<bool> IsInCustomerAsync(string userId, int customerId);
}