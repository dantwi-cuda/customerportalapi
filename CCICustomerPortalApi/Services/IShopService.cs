using CCICustomerPortalApi.Models.DTOs;

namespace CCICustomerPortalApi.Services;

public interface IShopService
{
    Task<ShopDto> GetShopByIdAsync(int id);
    Task<IEnumerable<ShopDto>> SearchShopsAsync(
        string? searchText,
        string? city,
        string? state,
        string? program,
        DateTime? startDate,
        DateTime? endDate);
    Task<ShopDto> CreateShopAsync(ShopDto shop);
    Task<ShopDto> UpdateShopAsync(int id, ShopDto shop);
    Task<bool> DeleteShopAsync(int id);
    Task SetShopActiveStatusAsync(int id, bool active);
    Task AssignProgramsAsync(int shopId, IEnumerable<int> programIds);
    Task AssignUsersAsync(int shopId, IEnumerable<string> userIds);
    Task<IEnumerable<ShopKpiDto>> GetShopKPIsAsync(int shopId);
}