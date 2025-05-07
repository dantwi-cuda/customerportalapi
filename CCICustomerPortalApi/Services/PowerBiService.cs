using Microsoft.Extensions.Options;
using Microsoft.Identity.Client;
using System.Net.Http.Headers;
using System.Text.Json;
using CCICustomerPortalApi.Models;

namespace CCICustomerPortalApi.Services;

public class PowerBiService : IPowerBiService
{
    private readonly PowerBiAuthenticationOptions _options;
    private readonly HttpClient _httpClient;
    private readonly ILogger<PowerBiService> _logger;
    private readonly ITenantService _tenantService;

    public PowerBiService(
        IOptions<PowerBiAuthenticationOptions> options,
        HttpClient httpClient,
        ILogger<PowerBiService> logger,
        ITenantService tenantService)
    {
        _options = options.Value;
        _httpClient = httpClient;
        _logger = logger;
        _tenantService = tenantService;
    }

    public async Task<string> GenerateEmbedTokenAsync(string reportId)
    {
        try
        {
            var accessToken = await GetAccessTokenAsync();
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

            var currentTenantId = _tenantService.GetCurrentTenantId();
            var requestUrl = $"https://api.powerbi.com/v1.0/myorg/groups/{_options.WorkspaceId}/reports/{reportId}/GenerateToken";

            var requestContent = new
            {
                accessLevel = "View",
                allowSaveAs = false,
                identities = new[]
                {
                    new
                    {
                        username = $"Customer_{currentTenantId}",
                        roles = new[] { "CustomerViewer" },
                        datasets = new[] { reportId }
                    }
                }
            };

            var response = await _httpClient.PostAsJsonAsync(requestUrl, requestContent);
            response.EnsureSuccessStatusCode();

            var result = await response.Content.ReadFromJsonAsync<JsonElement>();
            return result.GetProperty("token").GetString() ??
                throw new InvalidOperationException("Failed to get embed token from Power BI response");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating Power BI embed token for report {ReportId}", reportId);
            throw;
        }
    }

    public async Task<string> GetReportEmbedUrlAsync(string reportId)
    {
        try
        {
            var accessToken = await GetAccessTokenAsync();
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

            var requestUrl = $"https://api.powerbi.com/v1.0/myorg/groups/{_options.WorkspaceId}/reports/{reportId}";
            var response = await _httpClient.GetAsync(requestUrl);
            response.EnsureSuccessStatusCode();

            var result = await response.Content.ReadFromJsonAsync<JsonElement>();
            var embedUrl = result.GetProperty("embedUrl").GetString() ??
                throw new InvalidOperationException("Failed to get embed URL from Power BI response");

            // Append customer-specific filter if needed
            var currentTenantId = _tenantService.GetCurrentTenantId();
            return $"{embedUrl}&filter=Customer/Id eq {currentTenantId}";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting Power BI embed URL for report {ReportId}", reportId);
            throw;
        }
    }

    private async Task<string> GetAccessTokenAsync()
    {
        try
        {
            var app = ConfidentialClientApplicationBuilder
                .Create(_options.ClientId)
                .WithClientSecret(_options.ClientSecret)
                .WithAuthority($"{_options.AuthorityUri}/{_options.TenantId}")
                .Build();

            var scopes = new[] { $"{_options.ResourceUri}/.default" };
            var result = await app.AcquireTokenForClient(scopes).ExecuteAsync();
            return result.AccessToken;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error acquiring Power BI access token");
            throw;
        }
    }
}