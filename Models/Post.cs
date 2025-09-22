using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;


namespace DemoVolunteer.Models
{
    public class Post
    {
        [Key]
        public int PostId { get; set; }   //primary key

        public string Title { get; set; } = string.Empty; // ชื่อกิจกรรม
        public int? CategoryId { get; set; }           // ประเภทกิจกรรม
        public string Description { get; set; } = string.Empty; // ลายละเอียด
        public string Location { get; set; } = string.Empty;    // สถาที่นัด
        public int MaxParticipants { get; set; } // จำนวนมากสุดที่เข้าร่วม

        [DataType(DataType.Date)]
        public DateTime AppointmentDate { get; set; }              // เลือกวันที่จัด

        [DataType(DataType.Time)]
        public DateTime TimeStart { get; set; }              // เวลาเริ่ม

        [DataType(DataType.Time)]
        public DateTime TimeEnd { get; set; }              // เวลาจบ

        public int Score { get; set; }  // คะแนนกิจกรรมที่ได้รับ

        public string Status { get; set; } = string.Empty;  // Open / Closed / Full / Expired

        public bool IsActive { get; set; } = true;   // สถานะว่าแสดงไหม

        public string? OwnerId { get; set; }  // เจ้าของ post
        public string? ImgURL { get; set; }  // Path ของรูปภาพ


        public virtual ApplicationUser? Owner { get; set; }
        // public virtual User Owner { get; set; } = null!;  // เจ้าของ post
        public virtual Category? Category { get; set; } // ประเภทกิจกรรม
        public virtual ICollection<Join> Joins { get; set; } = new List<Join>(); //  ผู้ที่เข้าร่วมเก็บเป็น List


        // Computed property
        public string Time => $"{AppointmentDate} {TimeStart} - {TimeEnd}";
        public string AmountParticipant => $"{Joins.Count}";

        // เพิ่ม field ใหม่
        public DateTime? CreatedAt { get; set; }
        public DateTime? AppointmentDateEnd { get; set; }   // เลือกวันที่จัดวันสุดท้าย
        public string? AppointImg{ get; set; }   // เลือกรูปเข้ารวมกิจกรรม
    }
}