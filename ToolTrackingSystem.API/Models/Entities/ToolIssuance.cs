using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ToolTrackingSystem.API.Models.Entities
{
    public class ToolIssuance
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [ForeignKey("Tool")]
        public int ToolId { get; set; }

        [Required]
        [ForeignKey("IssuedTo")]
        public int IssuedToId { get; set; }

        [Required]
        [ForeignKey("IssuedBy")]
        public int IssuedById { get; set; }

        [Required]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public DateTime IssuedDate { get; set; } = DateTime.UtcNow;

        public DateTime? ExpectedReturnDate { get; set; }

        [MaxLength(500)]
        public string? Purpose { get; set; }

        // Changed to store as int but work with enum in code
        [Required]
        [Column("Status", TypeName = "int")]
        public int StatusValue { get; set; } = (int)IssuanceStatus.Issued;

        [NotMapped]
        public IssuanceStatus Status
        {
            get => (IssuanceStatus)StatusValue;
            set => StatusValue = (int)value;
        }

        [MaxLength(1000)]
        public string? Notes { get; set; }

        [Required]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        public virtual Tool Tool { get; set; } = null!;
        public virtual Employee IssuedTo { get; set; } = null!;
        public virtual User IssuedBy { get; set; } = null!;
        public virtual ToolReturn? ToolReturn { get; set; }
    }

    public enum IssuanceStatus
    {
        Issued = 1,
        Returned = 2,
        Overdue = 3,
        Lost = 4
    }
}