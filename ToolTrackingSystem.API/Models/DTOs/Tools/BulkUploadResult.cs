using System.ComponentModel.DataAnnotations;

namespace ToolTrackingSystem.API.Models.DTOs.Tools
{
    public class BulkUploadResult
    {
        [Required]
        public IFormFile File { get; set; }
        public int CreatedCount { get; set; }
        public int ErrorCount { get; set; }
        public List<string> Errors { get; set; } = new();
    }
}
