namespace ToolTrackingSystem.API.Models.DTOs.Dashboard
{
    public class RecentMovementDto
    {
        public string MovementType { get; set; } // "Checkout" or "Checkin"
        public string ToolName { get; set; }
        public string ToolCode { get; set; }
        public string UserName { get; set; }
        public string IssuanceNumber { get; set; }
        public DateTime MovementDate { get; set; }
        public string Status { get; set; }
    }
}
