namespace ToolTrackingSystem.API.Models.DTOs.Technicians
{
    public class CreateTechnicianDto
    {
        public string TechnicianId { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string? Email { get; set; }
        public string? Department { get; set; }
        public string? Position { get; set; }
        public bool IsActive { get; set; } = true;
    }
}
