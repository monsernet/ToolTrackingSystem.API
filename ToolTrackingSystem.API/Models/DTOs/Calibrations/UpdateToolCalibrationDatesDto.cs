namespace ToolTrackingSystem.API.Models.DTOs.Calibrations
{
    public class UpdateToolCalibrationDatesDto
    {
        public DateTime LastCalibrationDate { get; set; }
        public DateTime NextCalibrationDate { get; set; }
    }
}
