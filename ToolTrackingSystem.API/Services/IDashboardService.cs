using ToolTrackingSystem.API.Models.DTOs.Dashboard;

namespace ToolTrackingSystem.API.Services
{
    public interface IDashboardService
    {
        Task<DashboardStatsDto> GetDashboardStatsAsync();
        Task<IEnumerable<RecentMovementDto>> GetRecentMovementsAsync(int count = 5);
        Task<IEnumerable<OverdueIssuanceDto>> GetOverdueIssuancesAsync(bool includeBeingProcessed = false);
        Task<IEnumerable<CalibrationDueDto>> GetCalibrationsDueAsync();
    }
}
