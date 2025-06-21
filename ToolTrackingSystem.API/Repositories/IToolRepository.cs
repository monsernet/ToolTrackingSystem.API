using ToolTrackingSystem.API.Models.Entities;

namespace ToolTrackingSystem.API.Repositories
{
    public interface IToolRepository
    {
        Task<IEnumerable<Tool>> GetAllToolsAsync();
        Task<Tool?> GetToolByIdAsync(int id);
        Task AddToolAsync(Tool tool);
        Task UpdateToolAsync(Tool tool);
        Task DeleteToolAsync(int id);
        Task<bool> ToolExistsAsync(int id);
    }
}
