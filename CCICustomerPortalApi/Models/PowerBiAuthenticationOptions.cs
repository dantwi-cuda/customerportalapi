namespace CCICustomerPortalApi.Models;

public class PowerBiAuthenticationOptions
{
    public string WorkspaceId { get; set; } = null!;
    public string TenantId { get; set; } = null!;
    public string ClientId { get; set; } = null!;
    public string ClientSecret { get; set; } = null!;
    public string AuthorityUri { get; set; } = null!;
    public string ResourceUri { get; set; } = null!;
    public string EmbedUrlBase { get; set; } = null!;
}