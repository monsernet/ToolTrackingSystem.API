using ToolTrackingSystem.API.Models.DTOs.Calibrations;

namespace ToolTrackingSystem.API.Services
{
    public interface ICalibrationService
    {
        Task<IEnumerable<UpcomingCalibrationDto>> GetUpcomingCalibrationsAsync(int daysAhead);
        Task<IEnumerable<CalibrationRecordDto>> GetToolCalibrationHistoryAsync(int toolId);
        Task<CalibrationRecordDto> CreateCalibrationRecordAsync(CreateCalibrationDto dto);
        Task<bool> DeleteCalibrationRecordAsync(int id);
        Task<IEnumerable<UpcomingCalibrationDto>> GetOverdueCalibrationsAsync();

       
    }
}