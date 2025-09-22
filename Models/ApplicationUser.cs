using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;


namespace DemoVolunteer.Models
{
    public class ApplicationUser : IdentityUser
    {
        // สามารถเพิ่ม field เพิ่มเองได้ เช่น FirstName, LastName
        // เพิ่ม field อื่น ๆ ได้
        public string? FirstName { get; set; } = string.Empty;   // ชื่อ
        public string? LastName { get; set; } = string.Empty;    // นามสกุล

        // [Required]
        // [EmailAddress]
        // [StringLength(100)]
        // public string Email { get; set; } = string.Empty;       // อีเมล

        // [Required]
        // [StringLength(100)]
        // public string UserName { get; set; } = string.Empty;       // อีเมล

        public string? Gender { get; set; } = string.Empty;    // เพศ

        public DateTime? CreatedDate { get; set; } = DateTime.Now; // เวลาที่สร้าง user

        public int? Score { get; set; } = 0;                           // คะแนนจิตอาสา

        public bool? IsActive { get; set; } = true;


        public virtual ICollection<Join> Joins { get; set; } = new List<Join>();
        public virtual ICollection<Notification> Notifications { get; set; } = new List<Notification>();
        public virtual ICollection<Post> Posts { get; set; } = new List<Post>();

        // Computed property
        public string? FullName => $"{FirstName} {LastName}";


        //------------ new field
        public string? ImgURL { get; set; }
    }
}
