using Microsoft.AspNetCore.Authorization;

namespace CCICustomerPortalApi.Authorization;

public class MultiTenantRequirement : IAuthorizationRequirement
{
    // This is a marker class for our authorization requirement
}