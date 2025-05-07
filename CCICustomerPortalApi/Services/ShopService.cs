using AutoMapper;
using CCICustomerPortalApi.Data;
using CCICustomerPortalApi.Models;
using CCICustomerPortalApi.Models.DTOs;
using Microsoft.EntityFrameworkCore;

namespace CCICustomerPortalApi.Services;

public class ShopService : IShopService
{
    private readonly ApplicationDbContext _context;
    private readonly IMapper _mapper;
    private readonly ITenantService _tenantService;

    public ShopService(
        ApplicationDbContext context,
        IMapper mapper,
        ITenantService tenantService)
    {
        _context = context;
        _mapper = mapper;
        _tenantService = tenantService;
    }

    public async Task<ShopDto> GetShopByIdAsync(int id)
    {
        var shop = await _context.Shops
            .Include(s => s.ShopPrograms)
                .ThenInclude(sp => sp.Program)
            .Include(s => s.ShopKpis)
            .FirstOrDefaultAsync(s => s.Id == id);

        if (shop == null)
            throw new KeyNotFoundException($"Shop with ID {id} not found.");

        return _mapper.Map<ShopDto>(shop);
    }

    public async Task<IEnumerable<ShopDto>> SearchShopsAsync(
        string? searchText,
        string? city,
        string? state,
        string? program,
        DateTime? startDate,
        DateTime? endDate)
    {
        IQueryable<Shop> query = _context.Shops
            .Include(s => s.ShopPrograms)
                .ThenInclude(sp => sp.Program)
            .Include(s => s.ShopKpis);

        if (!string.IsNullOrWhiteSpace(searchText))
            query = query.Where(s => s.Name.Contains(searchText));

        if (!string.IsNullOrWhiteSpace(city))
            query = query.Where(s => s.City.Contains(city));

        if (!string.IsNullOrWhiteSpace(state))
            query = query.Where(s => s.State == state);

        if (!string.IsNullOrWhiteSpace(program))
            query = query.Where(s => s.ShopPrograms.Any(sp => sp.Program.Name.Contains(program)));

        // Date filtering can be applied to KPIs if needed
        if (startDate.HasValue)
            query = query.Where(s => s.ShopKpis.Any(k => k.Timestamp >= startDate.Value));

        if (endDate.HasValue)
            query = query.Where(s => s.ShopKpis.Any(k => k.Timestamp <= endDate.Value));

        var shops = await query.ToListAsync();
        return _mapper.Map<IEnumerable<ShopDto>>(shops);
    }

    public async Task<ShopDto> CreateShopAsync(ShopDto shopDto)
    {
        var shop = _mapper.Map<Shop>(shopDto);
        shop.CustomerId = _tenantService.GetCurrentTenantId();

        _context.Shops.Add(shop);
        await _context.SaveChangesAsync();

        return await GetShopByIdAsync(shop.Id);
    }

    public async Task<ShopDto> UpdateShopAsync(int id, ShopDto shopDto)
    {
        var shop = await _context.Shops.FindAsync(id);
        if (shop == null)
            throw new KeyNotFoundException($"Shop with ID {id} not found.");

        // Update main properties
        shop.Name = shopDto.Name;
        shop.Source = shopDto.Source;
        shop.PostalCode = shopDto.PostalCode;
        shop.City = shopDto.City;
        shop.State = shopDto.State;
        shop.Country = shopDto.Country;
        shop.IsActive = shopDto.IsActive;

        await _context.SaveChangesAsync();
        return await GetShopByIdAsync(id);
    }

    public async Task<bool> DeleteShopAsync(int id)
    {
        var shop = await _context.Shops.FindAsync(id);
        if (shop == null)
            return false;

        _context.Shops.Remove(shop);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task SetShopActiveStatusAsync(int id, bool active)
    {
        var shop = await _context.Shops.FindAsync(id);
        if (shop == null)
            throw new KeyNotFoundException($"Shop with ID {id} not found.");

        shop.IsActive = active;
        await _context.SaveChangesAsync();
    }

    public async Task AssignProgramsAsync(int shopId, IEnumerable<int> programIds)
    {
        var shop = await _context.Shops
            .Include(s => s.ShopPrograms)
            .FirstOrDefaultAsync(s => s.Id == shopId);

        if (shop == null)
            throw new KeyNotFoundException($"Shop with ID {shopId} not found.");

        // Remove existing programs
        _context.ShopPrograms.RemoveRange(shop.ShopPrograms);

        // Add new programs
        foreach (var programId in programIds)
        {
            shop.ShopPrograms.Add(new ShopProgram
            {
                ShopId = shopId,
                ProgramId = programId,
                AssignedAt = DateTime.UtcNow,
                IsActive = true
            });
        }

        await _context.SaveChangesAsync();
    }

    public async Task AssignUsersAsync(int shopId, IEnumerable<string> userIds)
    {
        var shop = await _context.Shops
            .Include(s => s.ShopUsers)
            .FirstOrDefaultAsync(s => s.Id == shopId);

        if (shop == null)
            throw new KeyNotFoundException($"Shop with ID {shopId} not found.");

        // Remove existing user assignments
        _context.ShopUsers.RemoveRange(shop.ShopUsers);

        // Add new user assignments
        foreach (var userId in userIds)
        {
            shop.ShopUsers.Add(new ShopUser
            {
                ShopId = shopId,
                UserId = userId,
                AssignedAt = DateTime.UtcNow
            });
        }

        await _context.SaveChangesAsync();
    }

    public async Task<IEnumerable<ShopKpiDto>> GetShopKPIsAsync(int shopId)
    {
        var shop = await _context.Shops
            .Include(s => s.ShopKpis)
            .FirstOrDefaultAsync(s => s.Id == shopId);

        if (shop == null)
            throw new KeyNotFoundException($"Shop with ID {shopId} not found.");

        return _mapper.Map<IEnumerable<ShopKpiDto>>(shop.ShopKpis);
    }
}