using Microsoft.EntityFrameworkCore;
using ToolTrackingSystem.API.Data;
using ToolTrackingSystem.API.Models.Entities;

namespace ToolTrackingSystem.API.Repositories
{
    public class CalibrationRepository : GenericRepository<ToolCalibration>, ICalibrationRepository
    {
        public CalibrationRepository(AppDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<ToolCalibration>> GetByToolIdAsync(int toolId)
        {
            return await _dbSet
                .Where(c => c.ToolId == toolId)
                .OrderByDescending(c => c.CalibrationDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<ToolCalibration>> GetUpcomingCalibrationsAsync(DateTime fromDate, DateTime toDate)
        {
            return await _dbSet
                .Where(c => c.NextCalibrationDate >= fromDate &&
                           c.NextCalibrationDate <= toDate)
                .Include(c => c.Tool)
                .OrderBy(c => c.NextCalibrationDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<ToolCalibration>> GetOverdueCalibrationsAsync(DateTime currentDate)
        {
            return await _dbSet
                .Where(c => c.NextCalibrationDate < currentDate)
                .Include(c => c.Tool)
                .OrderBy(c => c.NextCalibrationDate)
                .ToListAsync();
        }

        public async Task<bool> ToolHasCalibrationRecordsAsync(int toolId)
        {
            return await _dbSet
                .AnyAsync(c => c.ToolId == toolId);
        }

        
        public async Task<ToolCalibration?> GetMostRecentByToolIdAsync(int toolId)
        {
            return await _dbSet
                .Where(c => c.ToolId == toolId)
                .OrderByDescending(c => c.CalibrationDate)
                .FirstOrDefaultAsync();
        }
    }
}
