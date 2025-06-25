using Microsoft.EntityFrameworkCore;
using ToolTrackingSystem.API.Data;
using ToolTrackingSystem.API.Models.Entities;
using ToolTrackingSystem.API.Models.DTOs;
using ToolTrackingSystem.API.Models.DTOs.ToolIssuances;

namespace ToolTrackingSystem.API.Repositories
{
    public class IssuanceRepository : GenericRepository<ToolIssuance>, IIssuanceRepository
    {
        private readonly AppDbContext _context;

        public IssuanceRepository(AppDbContext context) : base(context)
        {
            _context = context;
        }

        #region Checkout Operations
        public async Task<ToolIssuance> CreateIssuanceAsync(ToolIssuance issuance)
        {
            await _context.ToolIssuances.AddAsync(issuance);
            await _context.SaveChangesAsync();
            return issuance;
        }

        public async Task<bool> ToolHasActiveIssuanceAsync(int toolId)
        {
            return await _context.ToolIssuances
                .AnyAsync(i => i.ToolId == toolId &&
                              i.StatusValue == (int)IssuanceStatus.Issued &&
                              i.ActualReturnDate == null);
        }

        public async Task<bool> TechnicianHasOverdueToolsAsync(int technicianId)
        {
            return await _context.ToolIssuances
                .AnyAsync(i => i.IssuedToId == technicianId &&
                              i.StatusValue == (int)IssuanceStatus.Overdue);
        }
        #endregion

        #region Checkin Operations
        public async Task<ToolIssuance?> CompleteIssuanceAsync(
            int issuanceId, 
            DateTime returnDate, 
            IssuanceStatus returnStatus, 
            int returnedById,
            ToolCondition condition,
            string? conditionNotes)
        {
            var issuance = await _context.ToolIssuances
                .Include(i => i.Tool)
                .FirstOrDefaultAsync(i => i.Id == issuanceId);

            if (issuance == null) return null;

            // 1- Update the issuance record
            issuance.ActualReturnDate = returnDate;
            issuance.StatusValue = (int)returnStatus;

            // 2. Create a return record
            var toolReturn = new ToolReturn
            {
                IssuanceId = issuanceId,
                ReturnedById = returnedById,
                Condition = condition,
                Notes = conditionNotes
            };
            await _context.ToolReturns.AddAsync(toolReturn);

            // 3. Update tool status
            if (issuance.Tool != null)
            {
                
                issuance.Tool.Status = returnStatus == IssuanceStatus.Damaged
                    ? ToolStatus.UnderMaintenance  // Damaged tools go to maintenance
                    : ToolStatus.Active;          // Otherwise mark as active
            }

            await _context.SaveChangesAsync();
            return issuance;
        }

        public async Task<int> BulkCompleteIssuancesAsync(IEnumerable<int> issuanceIds, DateTime returnDate)
        {
            var issuances = await _context.ToolIssuances
                .Where(i => issuanceIds.Contains(i.Id))
                .Include(i => i.Tool)
                .ToListAsync();

            foreach (var issuance in issuances)
            {
                issuance.ActualReturnDate = returnDate;
                issuance.Status = IssuanceStatus.Returned;

                if (issuance.Tool != null)
                {
                    issuance.Tool.Status = ToolStatus.Active;
                }
            }

            return await _context.SaveChangesAsync();
        }
        #endregion

        #region Query Operations
        public async Task<ToolIssuance?> GetActiveIssuanceForToolAsync(int toolId)
        {
            return await _context.ToolIssuances
                .Include(i => i.IssuedTo)
                .Include(i => i.IssuedBy)
                .FirstOrDefaultAsync(i => i.ToolId == toolId &&
                                       (i.StatusValue == (int)IssuanceStatus.Issued ||
                                        i.StatusValue == (int)IssuanceStatus.Overdue));
        }

        public async Task<IEnumerable<ToolIssuance>> GetActiveIssuancesAsync()
        {
            return await _context.ToolIssuances
                .Include(i => i.Tool)
                .Include(i => i.IssuedTo)
                .Where(i => i.StatusValue == (int)IssuanceStatus.Issued ||
                           i.StatusValue == (int)IssuanceStatus.Overdue)
                .OrderBy(i => i.ExpectedReturnDate)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<IEnumerable<ToolIssuance>> GetOverdueIssuancesAsync()
        {
            var now = DateTime.UtcNow;
            return await _context.ToolIssuances
                .Include(i => i.Tool)
                .Include(i => i.IssuedTo)
                .Where(i => i.StatusValue == (int)IssuanceStatus.Issued &&
                           i.ExpectedReturnDate.HasValue &&
                           i.ExpectedReturnDate < now)
                .ToListAsync();
        }

        public async Task<IEnumerable<ToolIssuance>> GetIssuanceHistoryForToolAsync(int toolId, int limit = 50)
        {
            return await _context.ToolIssuances
                .Include(i => i.IssuedTo)
                .Include(i => i.IssuedBy)
                .Where(i => i.ToolId == toolId)
                .OrderByDescending(i => i.IssuedDate)
                .Take(limit)
                .ToListAsync();
        }

        public async Task<IEnumerable<ToolIssuance>> GetIssuanceHistoryForTechnicianAsync(int technicianId, int daysBack = 30)
        {
            var cutoffDate = DateTime.UtcNow.AddDays(-daysBack);
            return await _context.ToolIssuances
                .Include(i => i.Tool)
                .Where(i => i.IssuedToId == technicianId &&
                            i.IssuedDate >= cutoffDate)
                .OrderByDescending(i => i.IssuedDate)
                .ToListAsync();
        }

        public async Task<IssuanceStatisticsDto> GetIssuanceStatisticsAsync()
        {
            var now = DateTime.UtcNow;
            var thirtyDaysAgo = now.AddDays(-30);

            return new IssuanceStatisticsDto
            {
                TotalActiveIssuances = await _context.ToolIssuances
                    .CountAsync(i => i.IsActiveIssuance()),
                OverdueIssuances = await _context.ToolIssuances
                    .CountAsync(i => i.Status == IssuanceStatus.Overdue),
                IssuancesLast30Days = await _context.ToolIssuances
                    .CountAsync(i => i.IssuedDate >= thirtyDaysAgo),
                AvgIssuanceDurationHours = await _context.ToolIssuances
                    .Where(i => i.ActualReturnDate.HasValue)
                    .AverageAsync(i => (i.ActualReturnDate!.Value - i.IssuedDate).TotalHours),
                ToolsInMaintenance = await _context.Tools
                    .CountAsync(t => t.Status == ToolStatus.UnderMaintenance)
            };
        }
        #endregion

        #region Utility Methods
        public async Task<bool> IsValidIssuanceNumberAsync(string issuanceNumber)
        {
            return !await _context.ToolIssuances
                .AnyAsync(i => i.IssuanceNumber == issuanceNumber);
        }

        public async Task<bool> ExistsAsync(int issuanceId)
        {
            return await _context.ToolIssuances
                .AnyAsync(i => i.Id == issuanceId);
        }
        #endregion
    }
}