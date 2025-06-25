using Microsoft.AspNetCore.Mvc;
using ToolTrackingSystem.API.Models.DTOs.Tools;
using ToolTrackingSystem.API.Models.Entities;

namespace ToolTrackingSystem.API.Repositories
{
    public interface ITechnicianRepository : IGenericRepository<Technician>
    {
        Task<IEnumerable<Technician>> GetActiveTechniciansAsync();
        Task<BulkUploadResult> ImportTechniciansFromExcelAsync(IFormFile file);
    }
}