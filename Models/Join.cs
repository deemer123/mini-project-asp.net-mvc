using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;


namespace DemoVolunteer.Models
{
    public class Join
    {

        [Key]
        public int JoinId { get; set; }

        public string? UserId { get; set; } // ต้องเป็น nullable ถ้าใช้ SetNull

        public int PostId { get; set; }

        [DataType(DataType.Date)]
        public DateTime JoinDate { get; set; }

        public string Status { get; set; } = "";


        public virtual ApplicationUser? User { get; set; }
        public virtual Post? Post { get; set; } = null!;
    }
}