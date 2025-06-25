using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ToolTrackingSystem.API.Models.Entities
{
    public class ToolReturn
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [ForeignKey("Issuance")]
        public int IssuanceId { get; set; }

        [Required]
        [ForeignKey("ReturnedBy")]
        public int ReturnedById { get; set; }

        [Required]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public DateTime ReturnedDate { get; set; } = DateTime.UtcNow;

        [Required]
        public ToolCondition Condition { get; set; }

        [MaxLength(1000)]
        public string? Notes { get; set; }

        [Required]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        public virtual ToolIssuance Issuance { get; set; } = null!;
        public virtual Technician ReturnedBy { get; set; } = null!;
    }

    public enum ToolCondition
    {
        Good = 1,
        Damaged = 2,
        NeedsMaintenance = 3
    }
}