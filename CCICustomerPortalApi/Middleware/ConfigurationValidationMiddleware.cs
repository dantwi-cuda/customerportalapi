using CCICustomerPortalApi.Models;
using CCICustomerPortalApi.Validators;
using FluentValidation;
using Microsoft.Extensions.Options;

namespace CCICustomerPortalApi.Middleware;

public class ConfigurationValidationMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ConfigurationValidationMiddleware> _logger;

    public ConfigurationValidationMiddleware(
        RequestDelegate next,
        ILogger<ConfigurationValidationMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(
        HttpContext context,
        IOptions<PowerBiAuthenticationOptions> powerBiOptions)
    {
        // Validate Power BI configuration on first request
        if (!context.Items.ContainsKey("PowerBIConfigValidated"))
        {
            var validator = new PowerBiAuthenticationOptionsValidator();
            var result = await validator.ValidateAsync(powerBiOptions.Value);

            if (!result.IsValid)
            {
                var errors = string.Join(Environment.NewLine, result.Errors.Select(e => e.ErrorMessage));
                _logger.LogError("Invalid Power BI configuration: {Errors}", errors);
                throw new InvalidOperationException("Invalid Power BI configuration. Check application logs for details.");
            }

            context.Items["PowerBIConfigValidated"] = true;
            _logger.LogInformation("Power BI configuration validated successfully");
        }

        await _next(context);
    }
}