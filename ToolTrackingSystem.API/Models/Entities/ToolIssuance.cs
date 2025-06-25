using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace ToolTrackingSystem.API.Models.Entities
{
    [Index(nameof(ToolId))]
    [Index(nameof(IssuedToId))]
    [Index(nameof(StatusValue))]
    [Index(nameof(IssuedDate))]
    [Index(nameof(IsOverdue))]
    [Index(nameof(LastOverdueNotificationDate))]
    public class ToolIssuance
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(20)]
        public string IssuanceNumber { get; set; } = GenerateIssuanceNumber();

        [Required]
        [ForeignKey("Tool")]
        public int ToolId { get; set; }

        [Required]
        [ForeignKey("IssuedTo")]
        public int IssuedToId { get; set; }

        [Required]
        [ForeignKey("IssuedBy")]
        public string IssuedById { get; set; }

        [Required]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public DateTime IssuedDate { get; set; } = DateTime.UtcNow;

        [Precision(0)]
        public DateTime? ExpectedReturnDate { get; set; }

        [Range(1, 365)]
        public int? ExpectedDurationDays { get; set; }

        [MaxLength(50)]
        public string? WorkOrderNumber { get; set; }

        [Required]
        [MaxLength(500)]
        public string Purpose { get; set; } = "General Use";

        // Enum stored as int but exposed as IssuanceStatus
        [Required]
        [Column("Status", TypeName = "int")]
        public int StatusValue { get; set; } = (int)IssuanceStatus.Issued;

        [NotMapped]
        public IssuanceStatus Status
        {
            get => (IssuanceStatus)StatusValue;
            set => StatusValue = (int)value;
        }

        [Precision(0)]
        public DateTime? ActualReturnDate { get; set; }

        [MaxLength(1000)]
        public string? Notes { get; set; }

        [Required]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Overdue Tracking Fields
        public bool IsOverdue { get; set; } = false;

        [Precision(0)]
        public DateTime? LastOverdueNotificationDate { get; set; }

        public int OverdueNotificationCount { get; set; } = 0;

        // Navigation properties
        public virtual Tool Tool { get; set; } = null!;
        public virtual Technician IssuedTo { get; set; } = null!;

        [ForeignKey("IssuedById")]
        public ApplicationUser IssuedBy { get; set; }
        public virtual ToolReturn? ToolReturn { get; set; }



        // Helper methods
        public bool IsActiveIssuance() =>
            StatusValue == (int)IssuanceStatus.Issued ||
            StatusValue == (int)IssuanceStatus.Overdue;

        public void MarkAsOverdue()
        {
            StatusValue = (int)IssuanceStatus.Overdue;
            IsOverdue = true;
            LastOverdueNotificationDate = DateTime.UtcNow;
            OverdueNotificationCount++;
        }

        public void CompleteIssuance(IssuanceStatus completionStatus)
        {
            StatusValue = (int)completionStatus;
            ActualReturnDate = DateTime.UtcNow;
            IsOverdue = false; // Reset overdue status
        }

        private static string GenerateIssuanceNumber()
        {
            return $"ISS-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString()[..4].ToUpper()}";
        }
    }

    public enum IssuanceStatus
    {
        Issued = 1,
        Returned = 2,
        Overdue = 3,
        Lost = 4,
        Damaged = 5,
        Maintenance = 6
    }
}