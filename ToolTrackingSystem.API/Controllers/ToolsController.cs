using ClosedXML.Excel;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ToolTrackingSystem.API.Models.DTOs.Tools;
using ToolTrackingSystem.API.Models.Entities;
using ToolTrackingSystem.API.Repositories;
using ClosedXML.Excel;
using ToolTrackingSystem.API.Models.DTOs.Calibrations;

namespace ToolTrackingSystem.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ToolsController : ControllerBase
    {
        private readonly IToolRepository _toolRepository;
        private readonly ILogger<ToolsController> _logger;

        public ToolsController(IToolRepository toolRepository, ILogger<ToolsController> logger)
        {
            _toolRepository = toolRepository;
            _logger = logger;
        }

        // GET: api/tools
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ToolDto>>> GetTools()
        {
            try
            {
                var tools = await _toolRepository.GetAllAsync();
                return Ok(tools.Select(MapToDto));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all tools");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpPost("bulk")]
        [Consumes("multipart/form-data")]
        [ProducesResponseType(typeof(BulkUploadResult), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> BulkUpload([FromForm] ToolBulkUploadRequestDTO request)
        {
            if (request?.File == null || request.File.Length == 0)
                return BadRequest("No file uploaded");

            if (!Path.GetExtension(request.File.FileName).Equals(".xlsx", StringComparison.OrdinalIgnoreCase))
                return BadRequest("Only .xlsx files are allowed");

            try
            {
                var result = await _toolRepository.ImportToolsFromExcelAsync(request.File);
                return Ok(result);
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogError(ex, "Invalid Excel format");
                return BadRequest("The uploaded Excel file is invalid or not properly formatted.");
            }
            catch (FormatException ex)
            {
                _logger.LogError(ex, "Excel contains format errors");
                return BadRequest("Excel contains invalid or misformatted values.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Bulk upload failed");

                return StatusCode(500, new
                {
                    error = ex.Message,
                    inner = ex.InnerException?.Message,
                    stackTrace = ex.StackTrace
                });
            }
        }


        // GET: api/tools/5
        [HttpGet("{id:int}")]
        public async Task<ActionResult<ToolDto>> GetTool(int id)
        {
            try
            {
                var tool = await _toolRepository.GetByIdAsync(id);

                if (tool == null)
                {
                    return NotFound();
                }

                return Ok(MapToDto(tool));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting tool with id {id}");
                return StatusCode(500, "Internal server error");
            }
        }

        // POST: api/tools
        [HttpPost]
        public async Task<ActionResult<ToolDto>> CreateTool([FromBody] CreateToolDto createDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var tool = new Tool
                {
                    Code = createDto.Code,
                    Name = createDto.Name,
                    Description = createDto.Description,
                    ToolType = createDto.ToolType,
                    Category = createDto.Category,
                    Unit = createDto.Unit,
                    StockQuantity = createDto.StockQuantity,
                    MinimumStock = createDto.MinimumStock,
                    CalibrationRequired = createDto.CalibrationRequired,
                    CalibrationFrequencyDays = createDto.CalibrationFrequencyDays,
                    LastCalibrationDate = createDto.LastCalibrationDate,
                    NextCalibrationDate = createDto.NextCalibrationDate,
                    Status = createDto.Status,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                await _toolRepository.AddAsync(tool);
                return CreatedAtAction(nameof(GetTool), new { id = tool.Id }, MapToDto(tool));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating tool");
                return StatusCode(500, "Internal server error");
            }
        }

        // PUT: api/tools/5
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateTool(int id, [FromBody] UpdateToolDto updateDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var tool = await _toolRepository.GetByIdAsync(id);
                if (tool == null)
                {
                    return NotFound();
                }

                tool.Code = updateDto.Code;
                tool.Name = updateDto.Name;
                tool.Description = updateDto.Description;
                tool.ToolType = updateDto.ToolType;
                tool.Category = updateDto.Category;
                tool.Unit = updateDto.Unit;
                tool.StockQuantity = updateDto.StockQuantity;
                tool.MinimumStock = updateDto.MinimumStock;
                tool.CalibrationRequired = updateDto.CalibrationRequired;
                tool.CalibrationFrequencyDays = updateDto.CalibrationFrequencyDays;
                tool.LastCalibrationDate = updateDto.LastCalibrationDate;
                tool.NextCalibrationDate = updateDto.NextCalibrationDate;
                tool.Status = updateDto.Status;
                tool.UpdatedAt = DateTime.UtcNow;

                await _toolRepository.UpdateAsync(tool);
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error updating tool with id {id}");
                return StatusCode(500, "Internal server error");
            }
        }

        // DELETE: api/tools/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTool(int id)
        {
            try
            {
                var tool = await _toolRepository.GetByIdAsync(id);
                if (tool == null)
                {
                    return NotFound();
                }

                await _toolRepository.DeleteAsync(tool);
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error deleting tool with id {id}");
                return StatusCode(500, "Internal server error");
            }
        }

        // GET: api/tools/type/{toolType}
        [HttpGet("type/{toolType}")]
        public async Task<ActionResult<IEnumerable<ToolDto>>> GetToolsByType(ToolType toolType)
        {
            try
            {
                var tools = await _toolRepository.GetToolsByTypeAsync(toolType);
                return Ok(tools.Select(MapToDto));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting tools by type {toolType}");
                return StatusCode(500, "Internal server error");
            }
        }

        // GET: api/tools/category/{category}
        [HttpGet("category/{category}")]
        public async Task<ActionResult<IEnumerable<ToolDto>>> GetToolsByCategory(string category)
        {
            try
            {
                var tools = await _toolRepository.GetToolsByCategoryAsync(category);
                return Ok(tools.Select(MapToDto));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting tools by category {category}");
                return StatusCode(500, "Internal server error");
            }
        }

        // GET: api/tools/low-stock
        [HttpGet("low-stock")]
        public async Task<ActionResult<IEnumerable<ToolDto>>> GetLowStockTools()
        {
            try
            {
                var tools = await _toolRepository.GetLowStockToolsAsync();
                return Ok(tools.Select(MapToDto));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting low stock tools");
                return StatusCode(500, "Internal server error");
            }
        }

        // GET: api/tools/search?term={searchTerm}
        [HttpGet("search")]
        public async Task<ActionResult<IEnumerable<ToolDto>>> SearchTools([FromQuery] string term)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(term))
                {
                    return BadRequest("Search term is required");
                }

                var tools = await _toolRepository.SearchToolsAsync(term);
                return Ok(tools.Select(MapToDto));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error searching tools with term {term}");
                return StatusCode(500, "Internal server error");
            }
        }

        // GET: api/tools/filter
        [HttpGet("filter")]
        public async Task<ActionResult<IEnumerable<ToolDto>>> FilterTools(
            [FromQuery] string? searchTerm = null,
            [FromQuery] ToolType? toolType = null,
            [FromQuery] string? category = null,
            [FromQuery] ToolStatus? status = null,
            [FromQuery] bool? calibrationRequired = null,
            [FromQuery] bool? lowStockOnly = false)
        {
            try
            {
                var tools = await _toolRepository.GetToolsByAdvancedFilterAsync(
                    searchTerm,
                    toolType,
                    category,
                    status,
                    calibrationRequired,
                    lowStockOnly);

                return Ok(tools.Select(MapToDto));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error filtering tools");
                return StatusCode(500, "Internal server error");
            }
        }

        // PATCH: api/tools/5/status
        [HttpPatch("{id}/status")]
        public async Task<IActionResult> UpdateToolStatus(int id, [FromBody] ToolStatus status)
        {
            try
            {
                await _toolRepository.UpdateToolStatusAsync(id, status);
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error updating status for tool {id}");
                return StatusCode(500, "Internal server error");
            }
        }

        // PATCH: api/tools/5/stock/increment
        [HttpPatch("{id}/stock/increment")]
        public async Task<IActionResult> IncrementStock(int id, [FromQuery] int quantity = 1)
        {
            try
            {
                if (quantity <= 0)
                {
                    return BadRequest("Quantity must be positive");
                }

                await _toolRepository.IncrementStockAsync(id, quantity);
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error incrementing stock for tool {id}");
                return StatusCode(500, "Internal server error");
            }
        }

        // PATCH: api/tools/5/stock/decrement
        [HttpPatch("{id}/stock/decrement")]
        public async Task<IActionResult> DecrementStock(int id, [FromQuery] int quantity = 1)
        {
            try
            {
                if (quantity <= 0)
                {
                    return BadRequest("Quantity must be positive");
                }

                await _toolRepository.DecrementStockAsync(id, quantity);
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error decrementing stock for tool {id}");
                return StatusCode(500, "Internal server error");
            }
        }

        // GET: api/tools/available
        [HttpGet("available")]
        public async Task<ActionResult<IEnumerable<ToolDto>>> GetAvailableTools()
        {
            try
            {
                var tools = await _toolRepository.GetAvailableToolsAsync();
                return Ok(tools.Select(MapToDto));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting available tools");
                return StatusCode(500, "Internal server error");
            }
        }

        private static ToolDto MapToDto(Tool tool)
        {
            return new ToolDto
            {
                Id = tool.Id,
                Code = tool.Code,
                Name = tool.Name,
                Description = tool.Description,
                ToolType = tool.ToolType,
                Category = tool.Category,
                Unit = tool.Unit,
                StockQuantity = tool.StockQuantity,
                MinimumStock = tool.MinimumStock,
                CalibrationRequired = tool.CalibrationRequired,
                CalibrationFrequencyDays = tool.CalibrationFrequencyDays,
                LastCalibrationDate = tool.LastCalibrationDate,
                NextCalibrationDate = tool.NextCalibrationDate,
                Status = tool.Status
            };
        }

        [HttpPatch("{id}/calibration-dates")]
        public async Task<IActionResult> UpdateToolCalibrationDates(int id, [FromBody] UpdateToolCalibrationDatesDto dto)
        {
            var tool = await _toolRepository.GetByIdAsync(id);
            if (tool == null) return NotFound();

            tool.LastCalibrationDate = dto.LastCalibrationDate;
            tool.NextCalibrationDate = dto.NextCalibrationDate;

            await _toolRepository.UpdateAsync(tool);
            return NoContent();
        }





}
}