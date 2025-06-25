using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using ToolTrackingSystem.API.Models.DTOs.Dashboard;
using ToolTrackingSystem.API.Services;

namespace ToolTrackingSystem.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    
    public class DashboardController : ControllerBase
    {
        private readonly IDashboardService _dashboardService;
        private readonly ILogger<DashboardController> _logger;

        public DashboardController(
            IDashboardService dashboardService,
            ILogger<DashboardController> logger)
        {
            _dashboardService = dashboardService;
            _logger = logger;
        }

        /// <summary>
        /// Gets dashboard statistics including tool counts and statuses
        /// </summary>
        
        [HttpGet("stats")]
        public async Task<IActionResult> GetDashboardStats()
        {
            try
            {
                _logger.LogInformation("Retrieving dashboard statistics");
                var stats = await _dashboardService.GetDashboardStatsAsync();
                return Ok(stats);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving dashboard statistics");
                return StatusCode(500, "An error occurred while retrieving dashboard statistics");
            }
        }

        /// <summary>
        /// Gets recent tool movements (checkouts and checkins)
        /// </summary>
        /// <param name="count">Number of recent movements to return (default: 5, max: 20)</param>
        [HttpGet("recent-movements")]
        [ProducesResponseType(typeof(IEnumerable<RecentMovementDto>), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> GetRecentMovements([FromQuery] int count = 5)
        {
            if (count <= 0 || count > 20)
            {
                return BadRequest("Count must be between 1 and 20");
            }

            try
            {
                var movements = await _dashboardService.GetRecentMovementsAsync(count);
                return Ok(movements);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving recent tool movements");
                return StatusCode(500, new ProblemDetails
                {
                    Title = "Server Error",
                    Detail = ex.Message,
                    Status = 500
                });
            }
        }

        /// <summary>
        /// Gets overdue tool issuances
        /// </summary>
        /// <param name="includeBeingProcessed">Whether to include items that are already being processed (default: false)</param>
        /// <response code="200">Returns the list of overdue issuances</response>
        /// <response code="401">If the user is not authenticated</response>
        /// <response code="500">If there was a server error</response>
        [HttpGet("overdue-issuances")]
        [ProducesResponseType(typeof(IEnumerable<OverdueIssuanceDto>), 200)]
        [ProducesResponseType(401)]
        [ProducesResponseType(typeof(ProblemDetails), 500)]
        public async Task<IActionResult> GetOverdueIssuances(
    [FromQuery] bool includeBeingProcessed = false)
        {
            try
            {
                var overdueIssuances = await _dashboardService
                    .GetOverdueIssuancesAsync(includeBeingProcessed);

                return Ok(overdueIssuances);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving overdue issuances");
                return StatusCode(500, new ProblemDetails
                {
                    Title = "Server Error",
                    Detail = ex.Message,
                    Status = 500
                });
            }
        }

        /// <summary>
        /// Gets tools with upcoming or overdue calibrations
        /// </summary>
        /// <response code="200">Returns the list of tools with due calibrations</response>
        /// <response code="401">If the user is not authenticated</response>
        /// <response code="500">If there was a server error</response>
        [HttpGet("calibrations-due")]
        [ProducesResponseType(typeof(IEnumerable<CalibrationDueDto>), 200)]
        [ProducesResponseType(401)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> GetCalibrationsDue()
        {
            try
            {
                _logger.LogInformation("Retrieving tools with due calibrations");
                var calibrations = await _dashboardService.GetCalibrationsDueAsync();
                return Ok(calibrations);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving tools with due calibrations");
                return StatusCode(500, "An error occurred while retrieving tools with due calibrations");
            }
        }

        /// <summary>
        /// Gets all dashboard data in a single request
        /// </summary>
        /// <param name="recentMovementsCount">Number of recent movements to include (default: 5)</param>
        /// <response code="200">Returns the complete dashboard data</response>
        /// <response code="400">If the recentMovementsCount parameter is invalid</response>
        /// <response code="401">If the user is not authenticated</response>
        /// <response code="500">If there was a server error</response>
        [HttpGet("all")]
        [Authorize]
        [ProducesResponseType(typeof(DashboardDataDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetAllDashboardData([FromQuery] int recentMovementsCount = 5)
        {
            try
            {
                // The [Authorize] attribute handles authentication automatically
                var stats = await _dashboardService.GetDashboardStatsAsync();
                var movements = await _dashboardService.GetRecentMovementsAsync(recentMovementsCount);
                var overdue = await _dashboardService.GetOverdueIssuancesAsync();
                var calibrations = await _dashboardService.GetCalibrationsDueAsync();

                return Ok(new DashboardDataDto
                {
                    Stats = stats,
                    RecentMovements = movements,
                    OverdueIssuances = overdue,
                    CalibrationsDue = calibrations
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetAllDashboardData");
                return StatusCode(500, new ProblemDetails
                {
                    Title = "Server Error",
                    Detail = ex.Message,
                    Status = 500
                });
            }
        }
    }
}