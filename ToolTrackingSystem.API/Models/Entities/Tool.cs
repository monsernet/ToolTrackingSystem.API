using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ToolTrackingSystem.API.Models.Entities
{
    public class Tool
    {
        [Key]
        public int Id { get; set; }

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
        public int StockQuantity { get; set; } = 1;

        [Required]
        [Range(0, int.MaxValue)]
        public int MinimumStock { get; set; } = 1;

        [Required]
        public bool CalibrationRequired { get; set; }

        [Range(1, int.MaxValue)]
        public int? CalibrationFrequencyDays { get; set; }

        public DateTime? LastCalibrationDate { get; set; }
        public DateTime? NextCalibrationDate { get; set; }

        [Required]
        public ToolStatus Status { get; set; } = ToolStatus.Active;

        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Required]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        public ICollection<ToolIssuance> Issuances { get; set; } = new List<ToolIssuance>();
        public ICollection<ToolCalibration> Calibrations { get; set; } = new List<ToolCalibration>();
    }

    public enum ToolType { Special = 1, DailyUse = 2 }
    public enum ToolStatus { Active = 1, Inactive = 2, UnderMaintenance = 3 }
}