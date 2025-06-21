using Microsoft.EntityFrameworkCore;
using ToolTrackingSystem.API.Data;
using ToolTrackingSystem.API.Models.Entities;

namespace ToolTrackingSystem.API.Repositories
{
    public class ToolRepository : IToolRepository
    {
        private readonly AppDbContext _context;

        public ToolRepository(AppDbContext appDbContext)
        {
            _context = appDbContext;
        }

        public async Task AddToolAsync (Tool tool)
        {
            await _context.Tools.AddAsync(tool);
            await _context.SaveChangesAsync();

        }

        public async Task DeleteToolAsync(int id)
        {
            var tool = await _context.Tools.FindAsync(id);
            if (tool!=null)
            {
                _context.Tools.Remove(tool);
                await _context.SaveChangesAsync();
            }

        }

        public async Task<IEnumerable<Tool>> GetAllToolsAsync()
        {
            var tools = await _context.Tools.ToListAsync();
            return tools;
        }

        public async Task<Tool?> GetToolByIdAsync(int id)
        {
            var tool = await _context.Tools.FindAsync(id);
            return tool;
        }

        public async Task<bool> ToolExistsAsync(int id)
        {
            return await _context.Tools.AnyAsync(e => e.Id == id);
        }

        async Task IToolRepository.UpdateToolAsync(Tool tool)
        {
            _context.Tools.Update(tool);
            await _context.SaveChangesAsync();
        }
    }
}
