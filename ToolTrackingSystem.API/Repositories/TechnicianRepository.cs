using ClosedXML.Excel;
using Microsoft.EntityFrameworkCore;
using ToolTrackingSystem.API.Data;
using ToolTrackingSystem.API.Models.DTOs.Tools;
using ToolTrackingSystem.API.Models.Entities;

namespace ToolTrackingSystem.API.Repositories
{
    public class TechnicianRepository : GenericRepository<Technician>, ITechnicianRepository
    {
        public TechnicianRepository(AppDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<Technician>> GetActiveTechniciansAsync()
        {
            return await _dbSet
                .Where(t => t.IsActive)
                .AsNoTracking()  // For read-only optimization
                .ToListAsync();
        }

        public async Task<BulkUploadResult> ImportTechniciansFromExcelAsync(IFormFile file)
        {
            var result = new BulkUploadResult();
            var technicians = new List<Technician>();

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
                    var technician = new Technician
                    {
                        TechnicianId = row.Cell(1).GetString().Trim(),
                        FirstName = row.Cell(2).GetString().Trim(),
                        LastName = row.Cell(3).GetString().Trim(),
                        Email = row.Cell(4).IsEmpty() ? null : row.Cell(4).GetString().Trim(),
                        Department = row.Cell(5).IsEmpty() ? null : row.Cell(5).GetString().Trim(),
                        Position = row.Cell(6).IsEmpty() ? null : row.Cell(6).GetString().Trim(),
                        IsActive = row.Cell(7).GetValue<bool>(),
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    };

                    // Validate required fields
                    if (string.IsNullOrWhiteSpace(technician.TechnicianId) ||
                        string.IsNullOrWhiteSpace(technician.FirstName) ||
                        string.IsNullOrWhiteSpace(technician.LastName))
                    {
                        throw new Exception("Required fields (TechnicianId, FirstName, LastName) cannot be empty");
                    }

                    technicians.Add(technician);
                    result.CreatedCount++;
                }
                catch (Exception ex)
                {
                    var error = $"Row {row.RowNumber()}: {ex.Message}";
                    result.ErrorCount++;
                    result.Errors.Add(error);
                }
            }

            // Save all technicians at once
            await AddRangeAsync(technicians);

            return result;
        }
    }
}