using System.ComponentModel.DataAnnotations;

namespace ToolTrackingSystem.API.Models.DTOs.Users
{
    public class UpdateUserDto
    {
        [Required]
        public string Id { get; set; }

        [Required, EmailAddress]
        public string Email { get; set; }

        [Required]
        public string FirstName { get; set; }

        [Required]
        public string LastName { get; set; }

        public string PhoneNumber { get; set; }

        [Required]
        public List<string> Roles { get; set; }
    }
}
