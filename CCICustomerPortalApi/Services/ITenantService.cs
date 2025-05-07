namespace CCICustomerPortalApi.Services;

public interface ITenantService
{
    int GetCurrentTenantId();
    void SetCurrentTenant(int tenantId);
    string GetCurrentSubdomain();
    void SetCurrentSubdomain(string subdomain);

    // New methods for admin portal functionality
    bool IsAdminPortalMode();
    void SetAdminPortalMode(bool isAdminPortal);
}