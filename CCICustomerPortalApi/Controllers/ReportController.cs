using CCICustomerPortalApi.Models.DTOs;
using CCICustomerPortalApi.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CCICustomerPortalApi.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ReportController : ControllerBase
{
    private readonly IReportService _reportService;

    public ReportController(IReportService reportService)
    {
        _reportService = reportService;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<ReportDto>>> GetReports([FromQuery] string? category = null)
    {
        var reports = await _reportService.GetReportsAsync(category);
        return Ok(reports);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ReportDto>> GetReport(int id)
    {
        try
        {
            var report = await _reportService.GetReportByIdAsync(id);
            return Ok(report);
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
    }

    [HttpGet("{id}/embed-config")]
    public async Task<ActionResult<PowerBiConfigDto>> GetReportEmbedConfig(int id)
    {
        try
        {
            var config = await _reportService.GetReportEmbedConfigAsync(id);
            return Ok(config);
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
        catch (NotImplementedException ex)
        {
            // This will be removed once Power BI integration is implemented
            return StatusCode(501, new { message = ex.Message });
        }
    }

    [Authorize(Roles = "Admin,CustomerAdmin")]
    [HttpPost]
    public async Task<ActionResult<ReportDto>> CreateReport([FromBody] ReportDto reportDto)
    {
        try
        {
            var report = await _reportService.CreateReportAsync(reportDto);
            return CreatedAtAction(nameof(GetReport), new { id = report.Id }, report);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [Authorize(Roles = "Admin,CustomerAdmin")]
    [HttpPut("{id}")]
    public async Task<ActionResult<ReportDto>> UpdateReport(int id, [FromBody] ReportDto reportDto)
    {
        try
        {
            var report = await _reportService.UpdateReportAsync(id, reportDto);
            return Ok(report);
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
    }

    [Authorize(Roles = "Admin,CustomerAdmin")]
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteReport(int id)
    {
        var result = await _reportService.DeleteReportAsync(id);
        if (!result)
            return NotFound();

        return NoContent();
    }
}