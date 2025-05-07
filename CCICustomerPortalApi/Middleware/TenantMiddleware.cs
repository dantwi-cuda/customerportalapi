using CCICustomerPortalApi.Data;
using CCICustomerPortalApi.Services;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace CCICustomerPortalApi.Middleware;

public class TenantMiddleware
{
    private readonly RequestDelegate _next;
    private readonly IWebHostEnvironment _environment;

    public TenantMiddleware(RequestDelegate next, IWebHostEnvironment environment)
    {
        _next = next;
        _environment = environment;
    }

    public async Task InvokeAsync(
        HttpContext context,
        ITenantService tenantService,
        ApplicationDbContext dbContext)
    {
        // Skip tenant resolution for auth endpoints
        if (context.Request.Path.StartsWithSegments("/api/auth"))
        {
            await _next(context);
            return;
        }

        // Check if tenant information is already in the JWT token claims
        if (context.User.Identity?.IsAuthenticated == true)
        {
            var tenantIdClaim = context.User.FindFirst("TenantId");
            var tenantSubdomainClaim = context.User.FindFirst("TenantSubdomain");

            // If both tenant claims exist in the token, use them instead of querying the database
            if (tenantIdClaim != null && tenantSubdomainClaim != null &&
                int.TryParse(tenantIdClaim.Value, out int tenantId))
            {
                tenantService.SetCurrentTenant(tenantId);
                tenantService.SetCurrentSubdomain(tenantSubdomainClaim.Value);
                await _next(context);
                return;
            }
        }

        // Fall back to the original subdomain-based resolution if JWT claims aren't available
        var host = context.Request.Host.Value;
        var subdomain = host.Split('.').First();

        // Special handling for localhost in development
        if (_environment.IsDevelopment() && (host.StartsWith("localhost") || host.StartsWith("127.0.0.1")))
        {
            // For development/localhost, either:
            // 1. Use a default tenant if one exists
            var defaultTenant = await dbContext.Customers.FirstOrDefaultAsync(c => c.IsActive);
            if (defaultTenant != null)
            {
                tenantService.SetCurrentTenant(defaultTenant.Id);
                tenantService.SetCurrentSubdomain(defaultTenant.Subdomain);
                await _next(context);
                return;
            }

            // 2. If no tenants exist yet in development, create a temporary tenant context
            tenantService.SetCurrentTenant(1); // Default ID for development
            tenantService.SetCurrentSubdomain("localhost");
            await _next(context);
            return;
        }

        // Normal tenant resolution for non-localhost environments
        var customer = await dbContext.Customers
            .FirstOrDefaultAsync(c => c.Subdomain == subdomain && c.IsActive);

        if (customer == null)
        {
            context.Response.StatusCode = 404;
            await context.Response.WriteAsJsonAsync(new { error = "Tenant not found" });
            return;
        }

        tenantService.SetCurrentTenant(customer.Id);
        tenantService.SetCurrentSubdomain(customer.Subdomain);

        await _next(context);
    }
}