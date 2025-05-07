using CCICustomerPortalApi.Services;

namespace CCICustomerPortalApi.Middleware;

// This special middleware is used only for development to bypass tenant resolution issues when running locally
public class DevTenantMiddleware
{
    private readonly RequestDelegate _next;
    private readonly IWebHostEnvironment _environment;
    private readonly bool _isDevelopment;

    public DevTenantMiddleware(RequestDelegate next, IWebHostEnvironment environment)
    {
        _next = next;
        _environment = environment;
        _isDevelopment = environment.IsDevelopment();
    }

    public async Task InvokeAsync(HttpContext context, ITenantService tenantService)
    {
        // Only apply in development mode
        if (_isDevelopment && (context.Request.Host.Value.StartsWith("localhost") || context.Request.Host.Value.StartsWith("127.0.0.1")))
        {
            // Set a default tenant ID for local development
            tenantService.SetCurrentTenant(1);
            tenantService.SetCurrentSubdomain("localhost");
        }

        // Continue the pipeline
        await _next(context);
    }
}