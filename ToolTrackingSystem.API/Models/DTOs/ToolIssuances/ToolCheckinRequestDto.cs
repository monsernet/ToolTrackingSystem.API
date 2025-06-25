using System.ComponentModel.DataAnnotations;

namespace ToolTrackingSystem.API.Models.DTOs.ToolIssuances
{
    public class ToolCheckinRequestDto
    {
        public bool IsDamaged { get; set; }
         
        public int ReturnedById { get; set; }

        [MaxLength(1000)]
        public string? ConditionNotes { get; set; }

        
    }
}
