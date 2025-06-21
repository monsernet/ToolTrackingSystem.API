using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ToolTrackingSystem.API.Models.Entities;
using ToolTrackingSystem.API.Repositories;

namespace ToolTrackingSystem.API.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class ToolsController : ControllerBase
    {
        private readonly IToolRepository _toolRepository;

        public ToolsController(IToolRepository toolRepository)
        {
            _toolRepository = toolRepository;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Tool>>> GetTools()
        {
            return Ok(await _toolRepository.GetAllToolsAsync());
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Tool>> GetTool(int id)
        {
            var tool = await _toolRepository.GetToolByIdAsync(id);
            return tool == null ? NotFound() : Ok(tool);
        }

        [HttpPost]
        public async Task<ActionResult<Tool>> PostTool( Tool tool)
        {
            await _toolRepository.AddToolAsync(tool);
            return CreatedAtAction(nameof(GetTool), new { id = tool.Id }, tool);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> PutTool(int id, Tool tool)
        {
            if (id != tool.Id) return BadRequest();

            if(! await _toolRepository.ToolExistsAsync(id))
            {
                return NotFound();
            }
            await _toolRepository.UpdateToolAsync(tool);
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTool (int id)
        {
            if (!await _toolRepository.ToolExistsAsync(id)) return NotFound();
            await _toolRepository.DeleteToolAsync(id);
            return NoContent();
        }
    }
}
