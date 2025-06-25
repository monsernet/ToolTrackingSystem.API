using Microsoft.EntityFrameworkCore;
using ToolTrackingSystem.API.Data;
using ToolTrackingSystem.API.Models.DTOs.Dashboard;
using ToolTrackingSystem.API.Models.Entities;

namespace ToolTrackingSystem.API.Services
{
    public class DashboardService : IDashboardService
    {
        private readonly AppDbContext _context;
        private readonly ILogger<DashboardService> _logger;

        public DashboardService(AppDbContext context, ILogger<DashboardService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<DashboardStatsDto> GetDashboardStatsAsync()
        {
            var stats = new DashboardStatsDto
            {
                TotalTools = await _context.Tools.CountAsync(),
                AvailableTools = await _context.Tools.CountAsync(t => t.Status == ToolStatus.Active),
                CheckedOutTools = await _context.ToolIssuances.CountAsync(i => i.ActualReturnDate == null),
                OverdueTools = await _context.ToolIssuances
                    .CountAsync(i => i.ActualReturnDate == null && i.ExpectedReturnDate < DateTime.UtcNow),
                CalibrationsDue = await _context.Tools
                    .CountAsync(c => c.NextCalibrationDate <= DateTime.UtcNow.AddDays(7)),
                MaintenanceRequired = await _context.Tools
                    .CountAsync(t => t.Status  == ToolStatus.UnderMaintenance)
            };

            return stats;
        }

        public async Task<IEnumerable<RecentMovementDto>> GetRecentMovementsAsync(int count = 5)
        {
            // Get recent checkouts (active issuances)
            var checkouts = await _context.ToolIssuances
                .Include(i => i.Tool)
                .Include(i => i.IssuedTo)
                .Where(i => i.ActualReturnDate == null &&
                           (i.StatusValue == (int)IssuanceStatus.Issued ||
                            i.StatusValue == (int)IssuanceStatus.Overdue))
                .OrderByDescending(i => i.IssuedDate)
                .Take(count)
                .Select(i => new RecentMovementDto
                {
                    MovementType = "Checkout",
                    ToolName = i.Tool.Name,
                    ToolCode = i.Tool.Code,
                    UserName = $"{i.IssuedTo.FirstName} {i.IssuedTo.LastName}",
                    MovementDate = i.IssuedDate,
                    Status = ((IssuanceStatus)i.StatusValue).ToString(),
                    IssuanceNumber = i.IssuanceNumber
                })
                .ToListAsync();

            // Get recent checkins (completed issuances)
            var checkins = await _context.ToolIssuances
                .Include(i => i.Tool)
                .Include(i => i.IssuedTo)
                .Where(i => i.StatusValue == (int)IssuanceStatus.Returned &&
                           i.ActualReturnDate.HasValue)
                .OrderByDescending(i => i.ActualReturnDate)
                .Take(count)
                .Select(i => new RecentMovementDto
                {
                    MovementType = "Checkin",
                    ToolName = i.Tool.Name,
                    ToolCode = i.Tool.Code,
                    UserName = $"{i.IssuedTo.FirstName} {i.IssuedTo.LastName}",
                    MovementDate = i.ActualReturnDate.Value,
                    Status = ((IssuanceStatus)i.StatusValue).ToString(),
                    IssuanceNumber = i.IssuanceNumber
                })
                .ToListAsync();

            return checkouts.Concat(checkins)
                .OrderByDescending(m => m.MovementDate)
                .Take(count)
                .ToList();
        }

        public async Task<IEnumerable<OverdueIssuanceDto>> GetOverdueIssuancesAsync(bool includeBeingProcessed = false)
        {
            // Convert enum values to integers for query
            var issuedStatus = (int)IssuanceStatus.Issued;
            var overdueStatus = (int)IssuanceStatus.Overdue;
            var currentDate = DateTime.UtcNow;

            var query = _context.ToolIssuances
                .Where(i => i.ActualReturnDate == null &&
                           i.ExpectedReturnDate.HasValue &&
                           i.ExpectedReturnDate < currentDate &&
                           (i.StatusValue == issuedStatus || i.StatusValue == overdueStatus));

            if (!includeBeingProcessed)
            {
                query = query.Where(i => i.LastOverdueNotificationDate == null ||
                                       i.LastOverdueNotificationDate < currentDate.AddDays(-1));
            }

            // Get the data with proper null checks
            var issuances = await query
                .Include(i => i.Tool)
                .Include(i => i.IssuedTo)
                .Include(i => i.IssuedBy)
                .OrderByDescending(i => i.ExpectedReturnDate)
                .Select(i => new
                {
                    Issuance = i,
                    ExpectedReturnDate = i.ExpectedReturnDate.Value
                })
                .ToListAsync();

            // Perform date calculations in memory
            return issuances
                .Select(x => new
                {
                    Data = x,
                    DaysOverdue = (int)(currentDate - x.ExpectedReturnDate).TotalDays
                })
                .OrderByDescending(x => x.DaysOverdue)
                .ThenByDescending(x => x.Data.Issuance.Tool.ToolType == ToolType.Special)
                .Select(x => new OverdueIssuanceDto
                {
                    IssuanceNumber = x.Data.Issuance.IssuanceNumber,
                    IssuedDate = x.Data.Issuance.IssuedDate,
                    DueDate = x.Data.ExpectedReturnDate,
                    DaysOverdue = x.DaysOverdue,
                    ToolName = x.Data.Issuance.Tool?.Name ?? "Unknown",
                    ToolCode = x.Data.Issuance.Tool?.Code ?? "N/A",
                    ToolStatus = x.Data.Issuance.Tool?.Status.ToString() ?? "Unknown",
                    ToolType = x.Data.Issuance.Tool?.ToolType.ToString() ?? "Unknown",
                    IsCriticalTool = x.Data.Issuance.Tool?.ToolType == ToolType.Special,
                    TechnicianName = x.Data.Issuance.IssuedTo != null ?
                        $"{x.Data.Issuance.IssuedTo.FirstName} {x.Data.Issuance.IssuedTo.LastName}" : "Unknown",
                    WorkOrderNumber = x.Data.Issuance.WorkOrderNumber,
                    Purpose = x.Data.Issuance.Purpose,
                    LastNotificationDate = x.Data.Issuance.LastOverdueNotificationDate,
                    NotificationCount = x.Data.Issuance.OverdueNotificationCount,
                    CurrentStatus = ((IssuanceStatus)x.Data.Issuance.StatusValue).ToString()
                })
                .ToList();
        }

        public async Task<IEnumerable<CalibrationDueDto>> GetCalibrationsDueAsync()
        {
            var currentDate = DateTime.UtcNow;

            var calibrationsDue = await _context.Tools
                .Where(t => t.CalibrationRequired &&
                           t.NextCalibrationDate != null &&
                           t.NextCalibrationDate <= currentDate.AddDays(7))
                .OrderBy(t => t.NextCalibrationDate)
                .Select(t => new CalibrationDueDto
                {
                    ToolName = t.Name,
                    ToolCode = t.Code,
                    CalibrationType = t.Calibrations
                        .OrderByDescending(c => c.CalibrationDate)
                        .FirstOrDefault()!
                        .CalibrationMethod, // Gets the most recent calibration method
                    LastCalibrationDate = (DateTime)t.LastCalibrationDate,
                    DueDate = (DateTime)t.NextCalibrationDate,
                    DaysUntilDue = (int)(t.NextCalibrationDate!.Value - currentDate).TotalDays
                })
                .ToListAsync();

            return calibrationsDue;
        }

        /*public async Task<IEnumerable<CalibrationDueDto>> GetCalibrationsDueAsync()
        {
            var calibrationsDue = await _context.ToolCalibrations
                .Where(c => c.NextCalibrationDate <= DateTime.UtcNow.AddDays(7))
                .Include(c => c.Tool)
                .OrderBy(c => c.NextCalibrationDate)
                .Select(c => new CalibrationDueDto
                {
                    ToolName = c.Tool.Name,
                    ToolCode = c.Tool.Code,
                    CalibrationType = c.CalibrationMethod,
                    LastCalibrationDate = c.CalibrationDate,
                    DueDate = c.NextCalibrationDate,
                    DaysUntilDue = (int)(c.NextCalibrationDate - DateTime.UtcNow).TotalDays
                })
                .ToListAsync();

            return calibrationsDue;
        }*/
    }
}
