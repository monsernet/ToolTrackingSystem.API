using ClosedXML.Excel;
using Microsoft.EntityFrameworkCore;
using ToolTrackingSystem.API.Data;
using ToolTrackingSystem.API.Models.DTOs.Tools;
using ToolTrackingSystem.API.Models.Entities;

namespace ToolTrackingSystem.API.Repositories
{
    public class ToolRepository : GenericRepository<Tool>, IToolRepository
    {
        

        public ToolRepository(AppDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<Tool>> GetToolsByTypeAsync(ToolType toolType)
        {
            return await _dbSet
                .Where(t => t.ToolType == toolType)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<IEnumerable<Tool>> GetToolsByCategoryAsync(string category)
        {
            return await _dbSet
                .Where(t => t.Category == category)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<IEnumerable<Tool>> GetToolsByStatusAsync(ToolStatus status)
        {
            return await _dbSet
                .Where(t => t.Status == status)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<IEnumerable<Tool>> GetLowStockToolsAsync()
        {
            return await _dbSet
                .Where(t => t.StockQuantity <= t.MinimumStock)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<int> GetTotalToolCountAsync()
        {
            return await _dbSet.CountAsync();
        }

        public async Task<IEnumerable<Tool>> SearchToolsAsync(string searchTerm)
        {
            return await _dbSet
                .Where(t => t.Name.Contains(searchTerm) ||
                           t.Code.Contains(searchTerm) ||
                           t.Description.Contains(searchTerm))
                .AsNoTracking()
                .Take(100) // Limit results
                .ToListAsync();
        }

        public async Task<IEnumerable<Tool>> GetToolsByAdvancedFilterAsync(
            string? searchTerm = null,
            ToolType? toolType = null,
            string? category = null,
            ToolStatus? status = null,
            bool? calibrationRequired = null,
            bool? lowStockOnly = false)
        {
            var query = _dbSet.AsQueryable();

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                query = query.Where(t => t.Name.Contains(searchTerm) ||
                                     t.Code.Contains(searchTerm) ||
                                     t.Description.Contains(searchTerm));
            }

            if (toolType.HasValue)
            {
                query = query.Where(t => t.ToolType == toolType.Value);
            }

            if (!string.IsNullOrWhiteSpace(category))
            {
                query = query.Where(t => t.Category == category);
            }

            if (status.HasValue)
            {
                query = query.Where(t => t.Status == status.Value);
            }

            if (calibrationRequired.HasValue)
            {
                query = query.Where(t => t.CalibrationRequired == calibrationRequired.Value);
            }

            if (lowStockOnly == true)
            {
                query = query.Where(t => t.StockQuantity <= t.MinimumStock);
            }

            return await query
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task UpdateToolStatusAsync(int toolId, ToolStatus status)
        {
            var tool = await _dbSet.FindAsync(toolId);
            if (tool != null)
            {
                tool.Status = status;
                tool.UpdatedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();
            }
        }

        public async Task IncrementStockAsync(int toolId, int quantity)
        {
            var tool = await _dbSet.FindAsync(toolId);
            if (tool != null)
            {
                tool.StockQuantity += quantity;
                tool.UpdatedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();
            }
        }

        public async Task<IEnumerable<Tool>> GetAvailableToolsAsync()
        {
            return await _context.Tools
                .Where(t => t.Status == ToolStatus.Active && t.StockQuantity > 0)
                .ToListAsync();
        }

        public async Task DecrementStockAsync(int toolId, int quantity)
        {
            var tool = await _dbSet.FindAsync(toolId);
            if (tool != null && tool.StockQuantity >= quantity)
            {
                tool.StockQuantity -= quantity;
                tool.UpdatedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();
            }
        }

        public async Task<BulkUploadResult> ImportToolsFromExcelAsync(IFormFile file)
        {
            var result = new BulkUploadResult();
            var tools = new List<Tool>();

            using var stream = new MemoryStream();
            await file.CopyToAsync(stream);
            stream.Position = 0;

            using var workbook = new XLWorkbook(stream);
            var worksheet = workbook.Worksheet(1);
            var rows = worksheet.RowsUsed().Skip(1); // Skip header

            foreach (var row in rows)
            {
                try
                {
                    var calibrationRequired = row.Cell(9).GetValue<bool>();
                    DateTime? lastCalibrationDate = row.Cell(11).IsEmpty() ? (DateTime?)null : row.Cell(11).GetDateTime();
                    DateTime? nextCalibrationDate = row.Cell(12).IsEmpty() ? (DateTime?)null : row.Cell(12).GetDateTime();

                    // Validation: If calibration is required, dates must be present
                    if (calibrationRequired && (lastCalibrationDate == null || nextCalibrationDate == null))
                    {
                        var error = $"Row {row.RowNumber()}: CalibrationRequired is true, but LastCalibrationDate or NextCalibrationDate is missing.";
                        result.ErrorCount++;
                        result.Errors.Add(error);
                        continue; // Skip saving this row
                    }

                    var tool = new Tool
                    {
                        Code = row.Cell(1).GetString().Trim(),
                        Name = row.Cell(2).GetString().Trim(),
                        Description = string.IsNullOrWhiteSpace(row.Cell(3).GetString()) ? null : row.Cell(3).GetString().Trim(),
                        ToolType = (ToolType)row.Cell(4).GetValue<int>(),
                        Category = string.IsNullOrWhiteSpace(row.Cell(5).GetString()) ? null : row.Cell(5).GetString().Trim(),
                        Unit = row.Cell(6).GetString().Trim(),
                        StockQuantity = row.Cell(7).GetValue<int>(),
                        MinimumStock = row.Cell(8).GetValue<int>(),
                        CalibrationRequired = row.Cell(9).GetValue<bool>(),
                        CalibrationFrequencyDays = row.Cell(10).GetValue<int?>(),
                        LastCalibrationDate = row.Cell(11).IsEmpty() ? (DateTime?)null : row.Cell(11).GetDateTime(),
                        NextCalibrationDate = row.Cell(12).IsEmpty() ? (DateTime?)null : row.Cell(12).GetDateTime(),
                        Status = (ToolStatus)row.Cell(13).GetValue<int>(),
                        CreatedAt = DateTime.UtcNow
                    };

                    tools.Add(tool);
                    result.CreatedCount++;
                }
                catch (Exception ex)
                {
                    var error = $"Row {row.RowNumber()}: {ex.Message}";
                    result.ErrorCount++;
                    result.Errors.Add(error);
                    //_logger.LogWarning(ex, error);
                }
            }

            // Save all tools at once
            await AddRangeAsync(tools);

            return result;
        }

    }
}