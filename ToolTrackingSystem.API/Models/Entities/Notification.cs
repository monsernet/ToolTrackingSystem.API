using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace ToolTrackingSystem.API.Models.Entities
{
        
    public class Notification
    {
        public int Id { get; set; }
        public string Message { get; set; }
        public int RecipientUserId { get; set; }
        public bool IsRead { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }

}
