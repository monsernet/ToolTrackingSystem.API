using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace ToolTrackingSystem.API.Models.Entities
{
    public class ApplicationUser : IdentityUser
    {
        [Required]
        [MaxLength(100)]
        public string FirstName { get; set; }

        [Required]
        [MaxLength(100)]
        public string LastName { get; set; }

        // Navigation properties
        public virtual ICollection<ToolIssuance> ToolIssuances { get; set; }
    }
}
