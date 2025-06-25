namespace ToolTrackingSystem.API.Models.DTOs.ToolIssuances
{
    public class IssuanceStatisticsDto
    {
        public int TotalActiveIssuances { get; set; }
        public int OverdueIssuances { get; set; }
        public int IssuancesLast30Days { get; set; }
        public double AvgIssuanceDurationHours { get; set; }
        public int ToolsInMaintenance { get; set; }
    }
}
