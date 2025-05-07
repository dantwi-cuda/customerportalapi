using CCICustomerPortalApi.Models.DTOs;

namespace CCICustomerPortalApi.Services;

public interface IReportService
{
    Task<IEnumerable<ReportDto>> GetReportsAsync(string? categoryName = null);
    Task<ReportDto> GetReportByIdAsync(int id);
    Task<PowerBiConfigDto> GetReportEmbedConfigAsync(int id);
    Task<ReportDto> CreateReportAsync(ReportDto report);
    Task<ReportDto> UpdateReportAsync(int id, ReportDto report);
    Task<bool> DeleteReportAsync(int id);
}