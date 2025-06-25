namespace ToolTrackingSystem.API.Models.DTOs.Calibrations
{
    public class UpcomingCalibrationDto
    {
        public int ToolId { get; set; }
        public string ToolCode { get; set; } = string.Empty;
        public string ToolName { get; set; } = string.Empty;
        public DateTime? LastCalibrationDate { get; set; }
        public DateTime NextCalibrationDate { get; set; }
        public int DaysUntilDue { get; set; }
    }
}
