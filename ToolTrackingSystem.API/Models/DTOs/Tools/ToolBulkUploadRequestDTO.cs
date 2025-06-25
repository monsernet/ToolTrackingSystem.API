using System.ComponentModel.DataAnnotations;

namespace ToolTrackingSystem.API.Models.DTOs.Tools
{
    public class ToolBulkUploadRequestDTO
    {
        [Required]
        public IFormFile File { get; set; }
    }
}
