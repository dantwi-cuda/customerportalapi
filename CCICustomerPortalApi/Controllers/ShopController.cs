using CCICustomerPortalApi.Models.DTOs;
using CCICustomerPortalApi.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CCICustomerPortalApi.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ShopController : ControllerBase
{
    public class ShopSearchParams
    {
        public string? SearchText { get; set; }
        public string? City { get; set; }
        public string? State { get; set; }
        public string? Program { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
    }

    public class AssignProgramsRequest
    {
        public List<int> ProgramIds { get; set; } = new();
    }

    public class AssignUsersRequest
    {
        public List<string> UserIds { get; set; } = new();
    }

    private readonly IShopService _shopService;

    public ShopController(IShopService shopService)
    {
        _shopService = shopService;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<ShopDto>>> SearchShops([FromQuery] ShopSearchParams searchParams)
    {
        var shops = await _shopService.SearchShopsAsync(
            searchParams.SearchText,
            searchParams.City,
            searchParams.State,
            searchParams.Program,
            searchParams.StartDate,
            searchParams.EndDate);

        return Ok(shops);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ShopDto>> GetShop(int id)
    {
        try
        {
            var shop = await _shopService.GetShopByIdAsync(id);
            return Ok(shop);
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
    }

    [Authorize(Roles = "Admin,CustomerAdmin")]
    [HttpPost]
    public async Task<ActionResult<ShopDto>> CreateShop([FromBody] ShopDto shopDto)
    {
        try
        {
            var shop = await _shopService.CreateShopAsync(shopDto);
            return CreatedAtAction(nameof(GetShop), new { id = shop.Id }, shop);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [Authorize(Roles = "Admin,CustomerAdmin")]
    [HttpPut("{id}")]
    public async Task<ActionResult<ShopDto>> UpdateShop(int id, [FromBody] ShopDto shopDto)
    {
        try
        {
            var shop = await _shopService.UpdateShopAsync(id, shopDto);
            return Ok(shop);
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [Authorize(Roles = "Admin,CustomerAdmin")]
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteShop(int id)
    {
        var result = await _shopService.DeleteShopAsync(id);
        if (!result)
            return NotFound();

        return NoContent();
    }

    [Authorize(Roles = "Admin,CustomerAdmin")]
    [HttpPost("{id}/activate")]
    public async Task<IActionResult> ActivateShop(int id)
    {
        try
        {
            await _shopService.SetShopActiveStatusAsync(id, true);
            return NoContent();
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
    }

    [Authorize(Roles = "Admin,CustomerAdmin")]
    [HttpPost("{id}/deactivate")]
    public async Task<IActionResult> DeactivateShop(int id)
    {
        try
        {
            await _shopService.SetShopActiveStatusAsync(id, false);
            return NoContent();
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
    }

    [Authorize(Roles = "Admin,CustomerAdmin")]
    [HttpPost("{id}/programs")]
    public async Task<IActionResult> AssignPrograms(int id, [FromBody] AssignProgramsRequest request)
    {
        try
        {
            await _shopService.AssignProgramsAsync(id, request.ProgramIds);
            return NoContent();
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [Authorize(Roles = "Admin,CustomerAdmin")]
    [HttpPost("{id}/users")]
    public async Task<IActionResult> AssignUsers(int id, [FromBody] AssignUsersRequest request)
    {
        try
        {
            await _shopService.AssignUsersAsync(id, request.UserIds);
            return NoContent();
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpGet("{id}/kpis")]
    public async Task<ActionResult<IEnumerable<ShopKpiDto>>> GetShopKPIs(int id)
    {
        try
        {
            var kpis = await _shopService.GetShopKPIsAsync(id);
            return Ok(kpis);
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
    }
}