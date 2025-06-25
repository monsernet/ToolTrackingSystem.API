using System.ComponentModel.DataAnnotations;
using ToolTrackingSystem.API.Models.Entities;

namespace ToolTrackingSystem.API.Models.DTOs.Tools
{
    public class UpdateToolDto
    {
        [Required]
        [MaxLength(50)]
        public string Code { get; set; } = string.Empty;

        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        [MaxLength(500)]
        public string? Description { get; set; }

        [Required]
        public ToolType ToolType { get; set; }

        [MaxLength(100)]
        public string? Category { get; set; }

        [MaxLength(50)]
        public string? Unit { get; set; }

        [Required]
        [Range(0, int.MaxValue)]
        public int StockQuantity { get; set; }

        [Required]
        [Range(0, int.MaxValue)]
        public int MinimumStock { get; set; }

        [Required]
        public bool CalibrationRequired { get; set; }

        [Range(1, int.MaxValue)]
        public int? CalibrationFrequencyDays { get; set; }

        public DateTime? LastCalibrationDate { get; set; }
        public DateTime? NextCalibrationDate { get; set; }

        [Required]
        public ToolStatus Status { get; set; }
    }
}
