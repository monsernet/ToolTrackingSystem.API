namespace ToolTrackingSystem.API.Models.DTOs.Users
{
    public class UserWithRoleDto
    {
        public string Id { get; set; }
        public string Email { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string UserName { get; set; }
        public string Role { get; set; }
    }
}
