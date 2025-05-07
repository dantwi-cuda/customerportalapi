namespace CCICustomerPortalApi.Models.DTOs;

public class PowerBiConfigDto
{
    public string EmbedToken { get; set; } = null!;
    public string EmbedUrl { get; set; } = null!;
    public string ReportId { get; set; } = null!;
    public int ExpiresInMinutes { get; set; }
}