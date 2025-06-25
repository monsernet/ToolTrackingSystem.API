using System.ComponentModel.DataAnnotations;

namespace ToolTrackingSystem.API.Models.DTOs.Calibrations
{
    public class CreateCalibrationDto
    {
        [Required]
        public int ToolId { get; set; }

        [Required]
        public string PerformedBy { get; set; } = string.Empty;

        public DateTime? CalibrationDate { get; set; } = DateTime.UtcNow;
        public string? CertificateNumber { get; set; }
        public string? Notes { get; set; }
        public string? CalibrationMethod { get; set; }
    }
}
