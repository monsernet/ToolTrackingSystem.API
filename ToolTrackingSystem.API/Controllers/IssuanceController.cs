using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using ToolTrackingSystem.API.Models.DTOs.ToolIssuances;
using ToolTrackingSystem.API.Models.Entities;
using ToolTrackingSystem.API.Repositories;
using System.Security.Claims;

namespace ToolTrackingSystem.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class IssuanceController : ControllerBase
    {
        private readonly IIssuanceRepository _issuanceRepo;
        private readonly IToolRepository _toolRepo;
        private readonly ITechnicianRepository _technicianRepo;
        private readonly ILogger<IssuanceController> _logger;

        public IssuanceController(
            IIssuanceRepository issuanceRepo,
            IToolRepository toolRepo,
            ITechnicianRepository technicianRepo,
            ILogger<IssuanceController> logger)
        {
            _issuanceRepo = issuanceRepo;
            _toolRepo = toolRepo;
            _technicianRepo = technicianRepo;
            _logger = logger;
        }

        [HttpPost("checkout")]
        //[Authorize(Roles = "Agent,Manager")]
        public async Task<IActionResult> CheckOutTool([FromBody] ToolCheckoutRequestDto request)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                if (request.ExpectedDurationDays < 0 )
                    return BadRequest("Expected duration must be greater than 0");

                

                var tool = await _toolRepo.GetByIdAsync(request.ToolId);
                if (tool == null)
                    return NotFound($"Tool with ID {request.ToolId} not found");

                if (tool.Status != ToolStatus.Active)
                    return BadRequest($"Tool is not available for checkout (Status: {tool.Status})");

                if (await _issuanceRepo.ToolHasActiveIssuanceAsync(request.ToolId))
                    return BadRequest("Tool is already checked out");

                var technician = await _technicianRepo.GetByIdAsync(request.TechnicianId);
                if (technician == null)
                    return NotFound($"Technician with ID {request.TechnicianId} not found");

                if (await _issuanceRepo.TechnicianHasOverdueToolsAsync(request.TechnicianId))
                    return BadRequest("Technician has overdue tools");

                // Get user ID from claims
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized("Invalid user credentials - no user ID found in token");
                }

                // Ensure dates are in UTC
                request.IssueDate = request.IssueDate.ToUniversalTime();
                if (request.ExpectedReturnDate.HasValue)
                {
                    request.ExpectedReturnDate = request.ExpectedReturnDate.Value.ToUniversalTime();
                }

                var issuance = new ToolIssuance
                {
                    ToolId = request.ToolId,
                    IssuedToId = request.TechnicianId,
                    IssuedById = userId, 
                    IssuedDate = request.IssueDate,
                    ExpectedReturnDate = request.ExpectedReturnDate,
                    ExpectedDurationDays = request.ExpectedDurationDays,
                    Purpose = request.Purpose,
                    Status = IssuanceStatus.Issued,
                    Notes = request.Notes
                }; 

                var createdIssuance = await _issuanceRepo.CreateIssuanceAsync(issuance);

                // Update tool status
                tool.Status = ToolStatus.Inactive;
                await _toolRepo.UpdateAsync(tool);

                return CreatedAtAction(nameof(GetIssuanceDetails),
                    new { id = createdIssuance.Id },
                    new IssuanceResponseDto(createdIssuance));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error during tool checkout for ToolId: {request?.ToolId}");
                return StatusCode(500, new
                {
                    Message = "Internal server error",
                    Exception = ex.Message,
                    StackTrace = ex.StackTrace,
                    InnerException = ex.InnerException?.Message
                });
            }
            /* catch (Exception ex)
             {
                 _logger.LogError(ex, "Error during tool checkout");
                 return StatusCode(500, "Internal server error");
             }
            */
        }

        [HttpPost("checkin/{toolId}")]
        public async Task<IActionResult> CheckInTool(int toolId, [FromBody] ToolCheckinRequestDto request)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                

                var activeIssuance = await _issuanceRepo.GetActiveIssuanceForToolAsync(toolId);
                if (activeIssuance == null)
                    return BadRequest($"No active issuance found for tool {toolId}");


                var condition = request.IsDamaged
                    ? ToolCondition.Damaged
                    : ToolCondition.Good;

                var completedIssuance = await _issuanceRepo.CompleteIssuanceAsync(
                    activeIssuance.Id,
                    DateTime.UtcNow,
                    request.IsDamaged ? IssuanceStatus.Damaged : IssuanceStatus.Returned,
                    request.ReturnedById,
                    condition,
                    request.ConditionNotes);

                if (completedIssuance == null)
                    return NotFound("Failed to complete issuance");

                return Ok(new
                {
                    Issuance = new IssuanceResponseDto(completedIssuance),
                    ReturnId = completedIssuance.ToolReturn?.Id
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error checking in tool {toolId}");
                return StatusCode(500, new ProblemDetails
                {
                    Title = "Server Error",
                    Detail = ex.Message,
                    Status = 500
                });
            }
            
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetIssuanceDetails(int id)
        {
            var issuance = await _issuanceRepo.GetByIdAsync(id);
            if (issuance == null)
                return NotFound();

            return Ok(new IssuanceResponseDto(issuance));
        }

        [HttpGet("active")]
        public async Task<IActionResult> GetActiveIssuances()
        {
            var issuances = await _issuanceRepo.GetActiveIssuancesAsync();
            return Ok(issuances.Select(i => new IssuanceResponseDto(i)));
        }

        [HttpGet("overdue")]
        public async Task<IActionResult> GetOverdueIssuances()
        {
            var issuances = await _issuanceRepo.GetOverdueIssuancesAsync();
            return Ok(issuances.Select(i => new IssuanceResponseDto(i)));
        }


    }
}