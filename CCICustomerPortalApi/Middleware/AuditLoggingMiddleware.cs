using CCICustomerPortalApi.Services;
using Microsoft.AspNetCore.Http.Extensions;
using System.Security.Claims;

namespace CCICustomerPortalApi.Middleware;

public class AuditLoggingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<AuditLoggingMiddleware> _logger;
    private readonly ITenantService _tenantService;

    public AuditLoggingMiddleware(
        RequestDelegate next,
        ILogger<AuditLoggingMiddleware> logger,
        ITenantService tenantService)
    {
        _next = next;
        _logger = logger;
        _tenantService = tenantService;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var startTime = DateTime.UtcNow;
        var originalBodyStream = context.Response.Body;

        try
        {
            using var memoryStream = new MemoryStream();
            context.Response.Body = memoryStream;

            // Get user info
            var userId = context.User?.FindFirstValue(ClaimTypes.NameIdentifier);
            var userEmail = context.User?.FindFirstValue(ClaimTypes.Email);
            var isCCIUser = context.User?.HasClaim(c => c.Type == "IsCCIUser" && c.Value == "true") ?? false;

            await _next(context);

            memoryStream.Position = 0;
            await memoryStream.CopyToAsync(originalBodyStream);

            // Log the request details
            var logData = new
            {
                Timestamp = startTime,
                UserId = userId,
                UserEmail = userEmail,
                IsCCIUser = isCCIUser,
                TenantId = _tenantService.GetCurrentTenantId(),
                Method = context.Request.Method,
                Path = context.Request.GetDisplayUrl(),
                StatusCode = context.Response.StatusCode,
                Duration = (DateTime.UtcNow - startTime).TotalMilliseconds
            };

            _logger.LogInformation("Audit Trail: {@AuditData}", logData);
        }
        finally
        {
            context.Response.Body = originalBodyStream;
        }
    }
}