using Microsoft.AspNetCore.Http;
using System.Threading;

namespace CCICustomerPortalApi.Services;

public class TenantService : ITenantService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    // Use AsyncLocal to store tenant context without requiring scoped lifetime
    // This allows safe concurrent access in a singleton service
    private readonly AsyncLocal<int?> _currentTenantId = new AsyncLocal<int?>();
    private readonly AsyncLocal<string> _currentSubdomain = new AsyncLocal<string>();
    private readonly AsyncLocal<bool> _isAdminPortalMode = new AsyncLocal<bool>();

    public TenantService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public int GetCurrentTenantId()
    {
        // If in admin portal mode, tenant ID is less relevant
        if (IsAdminPortalMode())
        {
            // Return a special value or the first tenant ID
            // depending on your admin portal's requirements
            return 0; // Special value for admin portal
        }

        if (_currentTenantId.Value.HasValue)
            return _currentTenantId.Value.Value;

        if (_httpContextAccessor.HttpContext?.Items["TenantId"] is int tenantId)
            return tenantId;

        // For development, return a default tenant ID
        if (IsLocalEnvironment())
            return 1;

        throw new InvalidOperationException("Tenant context is not set");
    }

    public void SetCurrentTenant(int tenantId)
    {
        _currentTenantId.Value = tenantId;
        if (_httpContextAccessor.HttpContext != null)
        {
            _httpContextAccessor.HttpContext.Items["TenantId"] = tenantId;
        }
    }

    public string GetCurrentSubdomain()
    {
        // If in admin portal mode, return a special value
        if (IsAdminPortalMode())
        {
            return "admin"; // Special value for admin portal
        }

        if (!string.IsNullOrEmpty(_currentSubdomain.Value))
            return _currentSubdomain.Value;

        if (_httpContextAccessor.HttpContext?.Items["Subdomain"] is string subdomain)
            return subdomain;

        // For development, return a default subdomain
        if (IsLocalEnvironment())
            return "localhost";

        throw new InvalidOperationException("Tenant subdomain is not set");
    }

    public void SetCurrentSubdomain(string subdomain)
    {
        _currentSubdomain.Value = subdomain;
        if (_httpContextAccessor.HttpContext != null)
        {
            _httpContextAccessor.HttpContext.Items["Subdomain"] = subdomain;
        }
    }

    public bool IsAdminPortalMode()
    {
        // Check AsyncLocal value first
        if (_isAdminPortalMode.Value)
            return true;

        // Then check HttpContext.Items
        if (_httpContextAccessor.HttpContext?.Items["IsAdminPortal"] is bool isAdmin)
            return isAdmin;

        // Default is false - not in admin portal mode
        return false;
    }

    public void SetAdminPortalMode(bool isAdminPortal)
    {
        _isAdminPortalMode.Value = isAdminPortal;
        if (_httpContextAccessor.HttpContext != null)
        {
            _httpContextAccessor.HttpContext.Items["IsAdminPortal"] = isAdminPortal;
        }
    }

    private bool IsLocalEnvironment()
    {
        var hostName = _httpContextAccessor.HttpContext?.Request?.Host.Value;
        return hostName != null && (hostName.StartsWith("localhost") || hostName.StartsWith("127.0.0.1"));
    }
}