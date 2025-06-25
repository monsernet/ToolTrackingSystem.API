using ToolTrackingSystem.API.Models.DTOs.Tools;
using ToolTrackingSystem.API.Models.Entities;

namespace ToolTrackingSystem.API.Repositories
{
    public interface IToolRepository : IGenericRepository<Tool>
    {
        // Inventory Management
        Task<IEnumerable<Tool>> GetToolsByTypeAsync(ToolType toolType);
        Task<IEnumerable<Tool>> GetToolsByCategoryAsync(string category);
        Task<IEnumerable<Tool>> GetToolsByStatusAsync(ToolStatus status);

        // Stock Management
        Task<IEnumerable<Tool>> GetLowStockToolsAsync();
        Task<int> GetTotalToolCountAsync();

        // Search & Filtering
        Task<IEnumerable<Tool>> SearchToolsAsync(string searchTerm);
        Task<IEnumerable<Tool>> GetToolsByAdvancedFilterAsync(
            string? searchTerm = null,
            ToolType? toolType = null,
            string? category = null,
            ToolStatus? status = null,
            bool? calibrationRequired = null,
            bool? lowStockOnly = false);

        // Status Updates
        Task UpdateToolStatusAsync(int toolId, ToolStatus status);
        Task IncrementStockAsync(int toolId, int quantity);
        Task DecrementStockAsync(int toolId, int quantity);
        Task<IEnumerable<Tool>> GetAvailableToolsAsync();

        Task<BulkUploadResult> ImportToolsFromExcelAsync(IFormFile file);
    }
}