using ToolTrackingSystem.API.Models.Entities;

namespace ToolTrackingSystem.API.Models.DTOs.Tools
{
    public class ToolDto
    {
        public int Id { get; set; }
        public string Code { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public ToolType ToolType { get; set; }
        public string? Category { get; set; }
        public string? Unit { get; set; }
        public int StockQuantity { get; set; }
        public int MinimumStock { get; set; }
        public bool CalibrationRequired { get; set; }
        public int? CalibrationFrequencyDays { get; set; }
        public DateTime? LastCalibrationDate { get; set; }
        public DateTime? NextCalibrationDate { get; set; }
        public ToolStatus? Status { get; set; }
    }
}
