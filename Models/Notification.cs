using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;


namespace DemoVolunteer.Models
{
    public class Notification
    {
        [Key]
        public int NotificationId { get; set; }
        public string? UserId { get; set; } // ต้องเป็น nullable ถ้าใช้ SetNull
        public string? Message { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public bool IsRead { get; set; }
        
        public virtual ApplicationUser? User { get; set; } = null!;
    }
}
