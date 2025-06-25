namespace ToolTrackingSystem.API.Models.DTOs.Dashboard
{
    public class DashboardDataDto
    {
        public DashboardStatsDto Stats { get; set; }
        public IEnumerable<RecentMovementDto> RecentMovements { get; set; }
        public IEnumerable<OverdueIssuanceDto> OverdueIssuances { get; set; }
        public IEnumerable<CalibrationDueDto> CalibrationsDue { get; set; }
    }
}
