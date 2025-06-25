using Microsoft.AspNetCore.Mvc;
using ToolTrackingSystem.API.Models.DTOs.Technicians;
using ToolTrackingSystem.API.Models.DTOs.Tools;
using ToolTrackingSystem.API.Models.Entities;
using ToolTrackingSystem.API.Repositories;

namespace ToolTrackingSystem.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TechniciansController : ControllerBase
    {
        private readonly ITechnicianRepository _technicianRepository;
        private readonly ILogger<TechniciansController> _logger;

        public TechniciansController(
            ITechnicianRepository technicianRepository,
            ILogger<TechniciansController> logger)
        {
            _technicianRepository = technicianRepository;
            _logger = logger;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<TechnicianDto>>> GetAllTechnicians()
        {
            try
            {
                var technicians = await _technicianRepository.GetAllAsync();
                return Ok(technicians.Select(MapToDto));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all technicians");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpGet("active")]
        public async Task<ActionResult<IEnumerable<TechnicianDto>>> GetActiveTechnicians()
        {
            try
            {
                var technicians = await _technicianRepository.GetActiveTechniciansAsync();
                return Ok(technicians.Select(MapToDto));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting active technicians");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<TechnicianDto>> GetTechnician(int id)
        {
            try
            {
                var technician = await _technicianRepository.GetByIdAsync(id);
                if (technician == null)
                {
                    return NotFound();
                }
                return Ok(MapToDto(technician));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting technician with id {id}");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpPost]
        public async Task<ActionResult<TechnicianDto>> CreateTechnician([FromBody] CreateTechnicianDto createDto)
        {
            _logger.LogInformation("Attempting to create technician");
            try
            {
                if (!ModelState.IsValid)
                {
                    _logger.LogWarning("Invalid model state: {@ModelState}", ModelState);
                    return BadRequest(ModelState);
                }

                _logger.LogDebug("Creating new technician entity");
                var technician = new Technician
                {
                    TechnicianId = createDto.TechnicianId,
                    FirstName = createDto.FirstName,
                    LastName = createDto.LastName,
                    Email = createDto.Email,
                    Department = createDto.Department,
                    Position = createDto.Position,
                    IsActive = createDto.IsActive,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                _logger.LogDebug("Adding technician to repository");
                await _technicianRepository.AddAsync(technician);
                _logger.LogInformation("Technician created with ID: {Id}", technician.Id);

                return CreatedAtAction(nameof(GetTechnician), new { id = technician.Id }, MapToDto(technician));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating technician");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpPost("bulk")]
        public async Task<ActionResult<BulkUploadResult>> BulkUploadTechnicians(IFormFile file)
        {
            try
            {
                if (file == null || file.Length == 0)
                {
                    return BadRequest("No file uploaded");
                }

                var result = await _technicianRepository.ImportTechniciansFromExcelAsync(file);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during bulk technician upload");
                return StatusCode(500, "Internal server error during bulk upload");
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateTechnician(int id, [FromBody] UpdateTechnicianDto updateDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var technician = await _technicianRepository.GetByIdAsync(id);
                if (technician == null)
                {
                    return NotFound();
                }

                technician.TechnicianId = updateDto.TechnicianId;
                technician.FirstName = updateDto.FirstName;
                technician.LastName = updateDto.LastName;
                technician.Email = updateDto.Email;
                technician.Department = updateDto.Department;
                technician.Position = updateDto.Position;
                technician.IsActive = updateDto.IsActive;
                technician.UpdatedAt = DateTime.UtcNow;

                await _technicianRepository.UpdateAsync(technician);
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error updating technician with id {id}");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTechnician(int id)
        {
            try
            {
                var technician = await _technicianRepository.GetByIdAsync(id);
                if (technician == null)
                {
                    return NotFound();
                }

                await _technicianRepository.DeleteAsync(technician);
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error deleting technician with id {id}");
                return StatusCode(500, "Internal server error");
            }
        }

        private static TechnicianDto MapToDto(Technician technician)
        {
            return new TechnicianDto
            {
                Id = technician.Id,
                TechnicianId = technician.TechnicianId,
                FirstName = technician.FirstName,
                LastName = technician.LastName,
                Email = technician.Email,
                Department = technician.Department,
                Position = technician.Position,
                IsActive = technician.IsActive
            };
        }
    }
}