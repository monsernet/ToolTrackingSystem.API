namespace ToolTrackingSystem.API.Models.DTOs.Dashboard
{
    public class OverdueIssuanceDto
    {
        // Basic Information
        public string IssuanceNumber { get; set; }
        public DateTime IssuedDate { get; set; }
        public DateTime DueDate { get; set; }
        public int DaysOverdue { get; set; }

        // Tool Information
        public string ToolName { get; set; }
        public string ToolCode { get; set; }
        public string ToolStatus { get; set; }
        public string ToolType { get; set; }
        public bool IsCriticalTool { get; set; }

        // Personnel Information
        public string TechnicianName { get; set; }
        public string TechnicianContact { get; set; } // If available
        public string SupervisorName { get; set; }
        public string SupervisorEmail { get; set; } // If available

        // Work Information
        public string WorkOrderNumber { get; set; }
        public string Purpose { get; set; }

        // Overdue Management
        public DateTime? LastNotificationDate { get; set; }
        public int NotificationCount { get; set; }
        public string CurrentStatus { get; set; } // "Overdue", "Warning", "Critical"

        // Calculated Properties (can be set in the service)
        public string OverdueSeverity
        {
            get
            {
                if (DaysOverdue > 14) return "Critical";
                if (DaysOverdue > 7) return "High";
                return "Medium";
            }
        }

        public bool NeedsImmediateAction
        {
            get { return IsCriticalTool || DaysOverdue > 7; }
        }

        // For UI display
        public string FormattedDueDate => DueDate.ToString("dd MMM yyyy");
        public string FormattedOverduePeriod
            => $"{DaysOverdue} day{(DaysOverdue != 1 ? "s" : "")} overdue";

    }
}
