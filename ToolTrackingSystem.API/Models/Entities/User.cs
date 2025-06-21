using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ToolTrackingSystem.API.Models.Entities
{
    public class User
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(50)]
        public string Username { get; set; } = string.Empty;

        [Required]
        [MaxLength(255)] // Adequate for hashed passwords
        public string PasswordHash { get; set; } = string.Empty;

        [Required]
        [MaxLength(100)] // Salt for password hashing
        public string Salt { get; set; } = string.Empty;

        [Required]
        [MaxLength(100)]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        public UserRole Role { get; set; }

        [ForeignKey("Employee")]
        public int? EmployeeId { get; set; } // Nullable for admin users who might not be employees

        [Required]
        public bool IsActive { get; set; } = true;

        public DateTime? LastLogin { get; set; }

        [Required]
        //[DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Required]
        //[DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        public virtual Employee? Employee { get; set; }
        public virtual ICollection<ToolIssuance> IssuedTools { get; set; } = new List<ToolIssuance>();
        public virtual ICollection<ToolReturn> ReturnedTools { get; set; } = new List<ToolReturn>();
        public virtual ICollection<ToolCalibration> PerformedCalibrations { get; set; } = new List<ToolCalibration>();
    }

    public enum UserRole
    {
        Admin = 1,
        Supervisor = 2,
        Staff = 3
    }
}