using Microsoft.AspNetCore.Mvc;
using ToolTrackingSystem.API.Models.DTOs.Calibrations;
using ToolTrackingSystem.API.Models.Entities;
using ToolTrackingSystem.API.Repositories;
using ToolTrackingSystem.API.Services;

namespace ToolTrackingSystem.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ToolCalibrationsController : ControllerBase
    {
        private readonly ICalibrationService _calibrationService;
        private readonly IGenericRepository<Tool> _toolRepository;

        public ToolCalibrationsController(
            ICalibrationService calibrationService,
            IGenericRepository<Tool> toolRepository)
        {
            _calibrationService = calibrationService;
            _toolRepository = toolRepository;
        }

        // GET: api/calibrations/upcoming?days=30
        [HttpGet("upcoming")]
        public async Task<ActionResult<IEnumerable<UpcomingCalibrationDto>>> GetUpcomingCalibrations(
            [FromQuery] int days = 30)
        {
            var calibrations = await _calibrationService.GetUpcomingCalibrationsAsync(days);
            return Ok(calibrations);
        }

        // GET: api/calibrations/tool/5
        [HttpGet("tool/{toolId}")]
        public async Task<ActionResult<IEnumerable<CalibrationRecordDto>>> GetToolCalibrationHistory(int toolId)
        {
            var history = await _calibrationService.GetToolCalibrationHistoryAsync(toolId);
            return Ok(history);
        }

        // POST: api/calibrations
        [HttpPost]
        public async Task<ActionResult<CalibrationRecordDto>> CreateCalibrationRecord(
            [FromBody] CreateCalibrationDto dto)
        {
            try
            {
                var record = await _calibrationService.CreateCalibrationRecordAsync(dto);
                return CreatedAtAction(
                    nameof(GetToolCalibrationHistory),
                    new { toolId = record.ToolId },
                    record);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        // DELETE: api/calibrations/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCalibrationRecord(int id)
        {
            var result = await _calibrationService.DeleteCalibrationRecordAsync(id);
            if (!result) return NotFound();
            return NoContent();
        }

        // GET: api/calibrations/overdue
        [HttpGet("overdue")]
        public async Task<ActionResult<IEnumerable<UpcomingCalibrationDto>>> GetOverdueCalibrations()
        {
            var overdue = await _calibrationService.GetOverdueCalibrationsAsync();
            return Ok(overdue);
        }

        [HttpGet("needs-calibration")]
        public async Task<ActionResult<IEnumerable<object>>> GetToolsRequiringCalibration()
        {
            var tools = await _toolRepository.GetAllAsync();
            return Ok(tools
                .Where(t => t.CalibrationRequired)
                .Select(t => new
                {
                    t.Id,
                    t.Code,
                    t.Name,
                    t.LastCalibrationDate,
                    t.NextCalibrationDate,
                    t.CalibrationFrequencyDays,
                    t.CalibrationRequired
                }));
        }

    }
}
