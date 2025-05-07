using CCICustomerPortalApi.Services;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace CCICustomerPortalApi.Authorization;

public class MultiTenantAuthorizationHandler : AuthorizationHandler<MultiTenantRequirement>
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ITenantService _tenantService;
    private readonly IUserService _userService;

    public MultiTenantAuthorizationHandler(
        IHttpContextAccessor httpContextAccessor,
        ITenantService tenantService,
        IUserService userService)
    {
        _httpContextAccessor = httpContextAccessor;
        _tenantService = tenantService;
        _userService = userService;
    }

    protected override async Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        MultiTenantRequirement requirement)
    {
        var user = context.User;
        if (!user.Identity?.IsAuthenticated ?? true)
        {
            return;
        }

        var userId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
        {
            return;
        }

        // CCI users have access to all tenants
        if (user.HasClaim(c => c.Type == "IsCCIUser" && c.Value == "true"))
        {
            context.Succeed(requirement);
            return;
        }

        // First check if tenant information is present in the JWT token claims
        var tenantIdClaim = user.FindFirst("TenantId");
        if (tenantIdClaim != null && int.TryParse(tenantIdClaim.Value, out int tokenTenantId))
        {
            // Get the current tenant ID from the tenant service
            var currentTenantId = _tenantService.GetCurrentTenantId();

            // Verify the tenant ID in the token matches the current tenant context
            if (tokenTenantId == currentTenantId)
            {
                // User has already been verified for this tenant during login
                context.Succeed(requirement);
                return;
            }
        }

        // Fallback to database check if tenant information is not in the token
        // or if the tenant context has changed since token was issued
        var currentTenantIdFallback = _tenantService.GetCurrentTenantId();
        var isInTenant = await _userService.IsInCustomerAsync(userId, currentTenantIdFallback);

        if (isInTenant)
        {
            context.Succeed(requirement);
        }
    }
}