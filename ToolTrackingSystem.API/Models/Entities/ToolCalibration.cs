using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ToolTrackingSystem.API.Models.Entities
{
    public class ToolCalibration
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [ForeignKey("Tool")]
        public int ToolId { get; set; }

        [Required]
        public DateTime CalibrationDate { get; set; }

        [Required]
        public DateTime NextCalibrationDate { get; set; }

        [Required]
        [MaxLength(100)]
        public string PerformedBy { get; set; }

        [MaxLength(100)]
        public string? CertificateNumber { get; set; }

        [MaxLength(1000)]
        public string? Notes { get; set; }

        [MaxLength(50)]
        public string? CalibrationMethod { get; set; } // "Laser Alignment", "Pressure Test"

        [MaxLength(100)]
        public string? StandardUsed { get; set; } // "ISO 6789", "Manufacturer Spec"

        [Required]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        public virtual Tool Tool { get; set; } = null!;
    }
}