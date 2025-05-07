namespace CCICustomerPortalApi.Models.DTOs;

public class UserDto
{
    public string Id { get; set; } = null!;
    public string Email { get; set; } = null!;
    public string? Name { get; set; }
    public string? Status { get; set; }
    public bool IsCustomerUser { get; set; }
    public bool IsCCIUser { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? LastLoginAt { get; set; }
    public List<string> Roles { get; set; } = new();
}