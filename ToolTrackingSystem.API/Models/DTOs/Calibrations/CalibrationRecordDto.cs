namespace ToolTrackingSystem.API.Models.DTOs.Calibrations
{
    public class CalibrationRecordDto
    {
        public int Id { get; set; }
        public int ToolId { get; set; }
        public string ToolCode { get; set; } = string.Empty;
        public string ToolName { get; set; } = string.Empty;
        public DateTime CalibrationDate { get; set; }
        public DateTime NextCalibrationDate { get; set; }
        public string PerformedBy { get; set; } = string.Empty;
        public string? CertificateNumber { get; set; }
        public string? Notes { get; set; }
        public string? CalibrationMethod { get; set; }
    }
}
