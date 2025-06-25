using ToolTrackingSystem.API.Models.DTOs.ToolIssuances;
using ToolTrackingSystem.API.Models.Entities;

namespace ToolTrackingSystem.API.Repositories
{
    public interface IIssuanceRepository : IGenericRepository<ToolIssuance>
    {
        // Checkout Operations
        Task<ToolIssuance> CreateIssuanceAsync(ToolIssuance issuance);
        Task<bool> ToolHasActiveIssuanceAsync(int toolId);
        Task<bool> TechnicianHasOverdueToolsAsync(int technicianId);

        // Checkin Operations
        Task<ToolIssuance?> CompleteIssuanceAsync(int issuanceId, DateTime returnDate, IssuanceStatus returnStatus, int returnedById, ToolCondition condition, string? conditionNotes);
        Task<int> BulkCompleteIssuancesAsync(IEnumerable<int> issuanceIds, DateTime returnDate);

        // Query Operations
        Task<ToolIssuance?> GetActiveIssuanceForToolAsync(int toolId);
        Task<IEnumerable<ToolIssuance>> GetActiveIssuancesAsync();
        Task<IEnumerable<ToolIssuance>> GetOverdueIssuancesAsync();
        Task<IEnumerable<ToolIssuance>> GetIssuanceHistoryForToolAsync(int toolId, int limit = 50);
        Task<IEnumerable<ToolIssuance>> GetIssuanceHistoryForTechnicianAsync(int technicianId, int daysBack = 30);
        Task<IssuanceStatisticsDto> GetIssuanceStatisticsAsync();

        // Utility Methods
        Task<bool> IsValidIssuanceNumberAsync(string issuanceNumber);
        //Task<bool> ExistsAsync(int issuanceId);
    }
}
