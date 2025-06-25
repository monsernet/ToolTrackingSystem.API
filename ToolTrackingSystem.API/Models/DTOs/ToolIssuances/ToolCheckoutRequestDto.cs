namespace ToolTrackingSystem.API.Models.DTOs.ToolIssuances
{
    public class ToolCheckoutRequestDto
    {
        public int ToolId { get; set; }
        public int TechnicianId { get; set; }
        public DateTime IssueDate { get; set; }  // Will parse ISO string automatically
        public DateTime? ExpectedReturnDate { get; set; }
        public int? ExpectedDurationDays { get; set; }
        public string Purpose { get; set; }
        public string? Notes { get; set; }

        /*public int ToolId { get; set; }
        public int TechnicianId { get; set; }
        public DateTime IssueDate { get; set; }
        public DateTime? ExpectedReturnDate { get; set; }
        public int? ExpectedDurationDays { get; set; }
        public string Purpose { get; set; } = string.Empty;
        public string? Notes { get; set; }
        */

    }
}
