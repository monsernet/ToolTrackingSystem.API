namespace ToolTrackingSystem.API.Models.DTOs.Dashboard
{
    public class DashboardStatsDto
    {
        public int TotalTools { get; set; }
        public int AvailableTools { get; set; }
        public int CheckedOutTools { get; set; }
        public int OverdueTools { get; set; }
        public int CalibrationsDue { get; set; }
        public int MaintenanceRequired { get; set; }
    }
}
