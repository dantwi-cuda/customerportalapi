namespace CCICustomerPortalApi.Services;

public interface IPowerBiService
{
    Task<string> GenerateEmbedTokenAsync(string reportId);
    Task<string> GetReportEmbedUrlAsync(string reportId);
}