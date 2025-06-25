namespace ToolTrackingSystem.API.Models.DTOs.Dashboard
{
    public class CalibrationDueDto
    {
        public string ToolName { get; set; }
        public string ToolCode { get; set; }
        public string CalibrationType { get; set; }
        public DateTime LastCalibrationDate { get; set; }
        public DateTime DueDate { get; set; }
        public int DaysUntilDue { get; set; }
    }
}
