using ToolTrackingSystem.API.Models.Entities;

namespace ToolTrackingSystem.API.Repositories
{
    public interface ICalibrationRepository : IGenericRepository<ToolCalibration>
    {
        Task<IEnumerable<ToolCalibration>> GetByToolIdAsync(int toolId);
        Task<IEnumerable<ToolCalibration>> GetUpcomingCalibrationsAsync(DateTime fromDate, DateTime toDate);
        Task<IEnumerable<ToolCalibration>> GetOverdueCalibrationsAsync(DateTime currentDate);
        Task<bool> ToolHasCalibrationRecordsAsync(int toolId);
    }
}
