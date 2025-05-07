using CCICustomerPortalApi.Data;
using CCICustomerPortalApi.Models;
using CCICustomerPortalApi.Models.DTOs;
using CCICustomerPortalApi.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace CCICustomerPortalApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IUserService _userService;
    private readonly IConfiguration _configuration;
    private readonly ApplicationDbContext _dbContext;

    public class LoginRequest
    {
        public string Email { get; set; } = null!;
        public string Password { get; set; } = null!;
        // Subdomain removed as it will be extracted from the HTTP request
    }

    public AuthController(
        UserManager<ApplicationUser> userManager,
        IUserService userService,
        IConfiguration configuration,
        ApplicationDbContext dbContext)
    {
        _userManager = userManager;
        _userService = userService;
        _configuration = configuration;
        _dbContext = dbContext;
    }

    [HttpPost("login")]
    public async Task<ActionResult<object>> Login([FromBody] LoginRequest request)
    {
        var user = await _userManager.FindByEmailAsync(request.Email);
        if (user == null || !await _userManager.CheckPasswordAsync(user, request.Password))
            return Unauthorized(new { message = "Invalid email or password" });

        // Initialize tenant information
        int? tenantId = null;
        string? tenantSubdomain = null;

        // For customer portal access, verify the customer association using the Host header
        if (user.IsCustomerUser)
        {
            // Extract subdomain from the request Host header
            string? subdomain = null;
            var host = HttpContext.Request.Host.Value;

            // Skip subdomain extraction for localhost in development
            var isDevelopmentEnvironment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Development";
            if (isDevelopmentEnvironment && (host.StartsWith("localhost") || host.StartsWith("127.0.0.1")))
            {
                // In development, use the first active customer for testing if needed
                var firstCustomer = await _dbContext.Customers.FirstOrDefaultAsync(c => c.IsActive);
                if (firstCustomer != null)
                {
                    subdomain = firstCustomer.Subdomain;
                }
            }
            else
            {
                // Extract subdomain from the hostname (e.g., "customer1.myapp.com" → "customer1")
                subdomain = host.Split('.').FirstOrDefault();
            }

            if (!string.IsNullOrEmpty(subdomain))
            {
                int customerId = await GetCustomerIdFromSubdomain(subdomain);
                if (customerId > 0)
                {
                    var isInCustomer = await _userService.IsInCustomerAsync(user.Id, customerId);
                    if (!isInCustomer)
                        return Unauthorized(new { message = "User not authorized for this customer portal" });

                    // Set tenant information to be included in the token
                    tenantId = customerId;
                    tenantSubdomain = subdomain;
                }
                else
                {
                    return NotFound(new { message = "Customer portal not found" });
                }
            }
        }

        var token = await GenerateJwtToken(user);
        user.LastLoginAt = DateTime.UtcNow;
        await _userManager.UpdateAsync(user);

        return new
        {
            token,
            user = await _userService.GetUserByIdAsync(user.Id)
        };
    }

    [Authorize]
    [HttpPost("change-password")]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest request)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
            return NotFound(new { message = "User not found" });

        var result = await _userManager.ChangePasswordAsync(user, request.CurrentPassword, request.NewPassword);
        if (!result.Succeeded)
            return BadRequest(new { message = "Failed to change password", errors = result.Errors });

        return Ok(new { message = "Password changed successfully" });
    }

    [Authorize(Roles = "Admin")]
    [HttpPost("reset-password/{userId}")]
    public async Task<IActionResult> ResetPassword(string userId, [FromBody] ResetPasswordRequest request)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
            return NotFound(new { message = "User not found" });

        var token = await _userManager.GeneratePasswordResetTokenAsync(user);
        var result = await _userManager.ResetPasswordAsync(user, token, request.NewPassword);

        if (!result.Succeeded)
            return BadRequest(new { message = "Failed to reset password", errors = result.Errors });

        return Ok(new { message = "Password reset successfully" });
    }

    private async Task<string> GenerateJwtToken(ApplicationUser user)
    {
        var roles = await _userManager.GetRolesAsync(user);
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id),
            new Claim(ClaimTypes.Email, user.Email!),
            new Claim("name", user.Name ?? user.Email!),
            new Claim("IsCustomerUser", user.IsCustomerUser.ToString()),
            new Claim("IsCCIUser", user.IsCCIUser.ToString())
        };

        // Check if this is a CCI admin user accessing from the main domain
        if (user.IsCCIUser && _tenantService.IsAdminPortalMode())
        {
            // Add admin portal claim to the token
            claims.Add(new Claim("IsAdminPortal", "true"));
        }
        // Otherwise, for customer users, add tenant information
        else if (user.IsCustomerUser)
        {
            // Add tenant information claims if available
            var tenantId = _tenantService.GetCurrentTenantId();
            var tenantSubdomain = _tenantService.GetCurrentSubdomain();

            if (tenantId > 0)
            {
                claims.Add(new Claim("TenantId", tenantId.ToString()));
            }

            if (!string.IsNullOrEmpty(tenantSubdomain))
            {
                claims.Add(new Claim("TenantSubdomain", tenantSubdomain));
            }
        }

        claims.AddRange(roles.Select(role => new Claim(ClaimTypes.Role, role)));

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Authentication:JwtKey"]!));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var expires = DateTime.UtcNow.AddMinutes(Convert.ToDouble(_configuration["Authentication:TokenLifetimeMinutes"]));

        var token = new JwtSecurityToken(
            issuer: _configuration["Authentication:Authority"],
            audience: _configuration["Authentication:Audience"],
            claims: claims,
            expires: expires,
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    private async Task<int> GetCustomerIdFromSubdomain(string subdomain)
    {
        var customer = await _dbContext.Customers
            .FirstOrDefaultAsync(c => c.Subdomain == subdomain && c.IsActive);

        return customer?.Id ?? 0;
    }

    public class ChangePasswordRequest
    {
        public string CurrentPassword { get; set; } = null!;
        public string NewPassword { get; set; } = null!;
    }

    public class ResetPasswordRequest
    {
        public string NewPassword { get; set; } = null!;
    }
}