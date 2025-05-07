using CCICustomerPortalApi.Data;
using CCICustomerPortalApi.Services;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace CCICustomerPortalApi.Middleware;

/// <summary>
/// Middleware to handle CCI admin portal access from the main domain
/// </summary>
public class AdminPortalMiddleware
{
    private readonly RequestDelegate _next;
    private readonly IConfiguration _configuration;
    private readonly IWebHostEnvironment _environment;

    public AdminPortalMiddleware(
        RequestDelegate next,
        IConfiguration configuration,
        IWebHostEnvironment environment)
    {
        _next = next;
        _configuration = configuration;
        _environment = environment;
    }

    public async Task InvokeAsync(
        HttpContext context,
        ITenantService tenantService)
    {
        // Skip processing for non-admin routes or authenticated API calls with existing tenant context
        if (ShouldSkipProcessing(context))
        {
            await _next(context);
            return;
        }

        // Check if this is the main domain (not a customer subdomain)
        var host = context.Request.Host.Value;
        var mainDomain = _configuration["Application:MainDomain"] ?? "yourdomain.com";

        bool isMainDomain = IsMainDomain(host, mainDomain);
        bool isAdminRoute = context.Request.Path.StartsWithSegments("/admin");

        // If accessing from main domain or explicit admin route, set admin context
        if (isMainDomain || isAdminRoute)
        {
            // For admin portal, we don't set a specific tenant
            // Instead, we mark the context as admin portal access
            tenantService.SetAdminPortalMode(true);

            // If the user is not authenticated and trying to access admin routes,
            // they'll be redirected to login by the authorization middleware
        }

        await _next(context);
    }

    private bool ShouldSkipProcessing(HttpContext context)
    {
        // Skip processing for authentication endpoints
        if (context.Request.Path.StartsWithSegments("/api/auth"))
        {
            return true;
        }

        // Skip processing if tenant context is already established in an authenticated request
        if (context.User.Identity?.IsAuthenticated == true &&
            context.User.HasClaim(c => c.Type == "TenantId" || c.Type == "IsAdminPortal"))
        {
            return true;
        }

        return false;
    }

    private bool IsMainDomain(string host, string mainDomain)
    {
        // Special case for localhost in development
        if (_environment.IsDevelopment() && (host.StartsWith("localhost") || host.StartsWith("127.0.0.1")))
        {
            // Check if an admin path or flag is present to differentiate between
            // regular development mode and admin development mode
            return true;
        }

        // For production, check if this is the exact main domain (no subdomain)
        // This assumes format like "yourdomain.com" or "admin.yourdomain.com"
        var hostParts = host.Split('.');

        if (hostParts.Length == 2)
        {
            // No subdomain, might be main domain (e.g., "yourdomain.com")
            return host.Equals(mainDomain, StringComparison.OrdinalIgnoreCase);
        }
        else if (hostParts.Length > 2 && hostParts[0].Equals("admin", StringComparison.OrdinalIgnoreCase))
        {
            // Has "admin" subdomain (e.g., "admin.yourdomain.com")
            return true;
        }

        return false;
    }
}