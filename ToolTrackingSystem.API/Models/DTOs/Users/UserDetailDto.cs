namespace ToolTrackingSystem.API.Models.DTOs.Users
{
    public class UserDetailDto : UserDto
    {
        public string PhoneNumber { get; set; }
        public List<string> Roles { get; set; }
    }
}
