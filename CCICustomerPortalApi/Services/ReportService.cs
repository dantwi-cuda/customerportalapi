using AutoMapper;
using CCICustomerPortalApi.Data;
using CCICustomerPortalApi.Models;
using CCICustomerPortalApi.Models.DTOs;
using Microsoft.EntityFrameworkCore;

namespace CCICustomerPortalApi.Services;

public class ReportService : IReportService
{
    private readonly ApplicationDbContext _context;
    private readonly IMapper _mapper;
    private readonly ITenantService _tenantService;
    private readonly IConfiguration _configuration;
    private readonly IPowerBiService _powerBiService;

    public ReportService(
        ApplicationDbContext context,
        IMapper mapper,
        ITenantService tenantService,
        IConfiguration configuration,
        IPowerBiService powerBiService)
    {
        _context = context;
        _mapper = mapper;
        _tenantService = tenantService;
        _configuration = configuration;
        _powerBiService = powerBiService;
    }

    public async Task<IEnumerable<ReportDto>> GetReportsAsync(string? categoryName = null)
    {
        IQueryable<Report> query = _context.Reports
            .Include(r => r.Category)
            .Include(r => r.Workspace);

        if (!string.IsNullOrWhiteSpace(categoryName))
        {
            query = query.Where(r => r.Category.Name.Contains(categoryName));
        }

        var reports = await query.ToListAsync();
        return _mapper.Map<IEnumerable<ReportDto>>(reports);
    }

    public async Task<ReportDto> GetReportByIdAsync(int id)
    {
        var report = await _context.Reports
            .Include(r => r.Category)
            .Include(r => r.Workspace)
            .FirstOrDefaultAsync(r => r.Id == id);

        if (report == null)
            throw new KeyNotFoundException($"Report with ID {id} not found.");

        return _mapper.Map<ReportDto>(report);
    }

    public async Task<PowerBiConfigDto> GetReportEmbedConfigAsync(int id)
    {
        var report = await _context.Reports.FindAsync(id);
        if (report == null)
            throw new KeyNotFoundException($"Report with ID {id} not found.");

        var embedToken = await _powerBiService.GenerateEmbedTokenAsync(report.PowerBiReportId);
        var embedUrl = await _powerBiService.GetReportEmbedUrlAsync(report.PowerBiReportId);

        return new PowerBiConfigDto
        {
            ReportId = report.PowerBiReportId,
            EmbedToken = embedToken,
            EmbedUrl = embedUrl,
            ExpiresInMinutes = 60
        };
    }

    public async Task<ReportDto> CreateReportAsync(ReportDto reportDto)
    {
        var report = _mapper.Map<Report>(reportDto);
        var category = await _context.ReportCategories.FindAsync(report.ReportCategoryId);

        if (category == null)
            throw new InvalidOperationException($"Report category with ID {report.ReportCategoryId} not found.");

        _context.Reports.Add(report);
        await _context.SaveChangesAsync();

        return await GetReportByIdAsync(report.Id);
    }

    public async Task<ReportDto> UpdateReportAsync(int id, ReportDto reportDto)
    {
        var report = await _context.Reports.FindAsync(id);
        if (report == null)
            throw new KeyNotFoundException($"Report with ID {id} not found.");

        report.Name = reportDto.Name;
        report.Description = reportDto.Description;
        report.ReportCategoryId = reportDto.ReportCategoryId;

        await _context.SaveChangesAsync();
        return await GetReportByIdAsync(id);
    }

    public async Task<bool> DeleteReportAsync(int id)
    {
        var report = await _context.Reports.FindAsync(id);
        if (report == null)
            return false;

        _context.Reports.Remove(report);
        await _context.SaveChangesAsync();
        return true;
    }
}