using CCICustomerPortalApi.Models.DTOs;
using CCICustomerPortalApi.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CCICustomerPortalApi.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class UserController : ControllerBase
{
    private readonly IUserService _userService;
    private readonly ITenantService _tenantService;

    public class CreateUserRequest
    {
        public UserDto User { get; set; } = null!;
        public string Password { get; set; } = null!;
        public List<string> Roles { get; set; } = new();
    }

    public class UpdateUserRequest
    {
        public UserDto User { get; set; } = null!;
        public List<string> Roles { get; set; } = new();
    }

    public UserController(IUserService userService, ITenantService tenantService)
    {
        _userService = userService;
        _tenantService = tenantService;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<UserDto>>> GetUsers(
        [FromQuery] bool? isCustomerUser,
        [FromQuery] bool? isCCIUser)
    {
        var users = await _userService.GetUsersAsync(isCustomerUser, isCCIUser);
        return Ok(users);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<UserDto>> GetUser(string id)
    {
        try
        {
            var user = await _userService.GetUserByIdAsync(id);
            return Ok(user);
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
    }

    [Authorize(Roles = "Admin,CustomerAdmin")]
    [HttpPost]
    public async Task<ActionResult<UserDto>> CreateUser([FromBody] CreateUserRequest request)
    {
        // Ensure CustomerAdmin can only create customer users
        if (User.IsInRole("CustomerAdmin") && !request.User.IsCustomerUser)
            return Forbid();

        try
        {
            var user = await _userService.CreateUserAsync(request.User, request.Password, request.Roles);

            // If creating a customer user, automatically assign to current tenant
            if (user.IsCustomerUser)
            {
                await _userService.AssignToCustomerAsync(user.Id, _tenantService.GetCurrentTenantId());
            }

            return CreatedAtAction(nameof(GetUser), new { id = user.Id }, user);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [Authorize(Roles = "Admin,CustomerAdmin")]
    [HttpPut("{id}")]
    public async Task<ActionResult<UserDto>> UpdateUser(string id, [FromBody] UpdateUserRequest request)
    {
        try
        {
            // Ensure CustomerAdmin can only update users in their tenant
            if (User.IsInRole("CustomerAdmin"))
            {
                var isInTenant = await _userService.IsInCustomerAsync(id, _tenantService.GetCurrentTenantId());
                if (!isInTenant)
                    return Forbid();
            }

            var user = await _userService.UpdateUserAsync(id, request.User);
            await _userService.UpdateUserRolesAsync(id, request.Roles);

            return Ok(user);
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [Authorize(Roles = "Admin")]
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteUser(string id)
    {
        var result = await _userService.DeleteUserAsync(id);
        if (!result)
            return NotFound();

        return NoContent();
    }

    [Authorize(Roles = "Admin")]
    [HttpPost("{id}/customers/{customerId}")]
    public async Task<IActionResult> AssignToCustomer(string id, int customerId)
    {
        var result = await _userService.AssignToCustomerAsync(id, customerId);
        if (!result)
            return BadRequest();

        return NoContent();
    }

    [Authorize(Roles = "Admin")]
    [HttpDelete("{id}/customers/{customerId}")]
    public async Task<IActionResult> RemoveFromCustomer(string id, int customerId)
    {
        var result = await _userService.RemoveFromCustomerAsync(id, customerId);
        if (!result)
            return NotFound();

        return NoContent();
    }
}